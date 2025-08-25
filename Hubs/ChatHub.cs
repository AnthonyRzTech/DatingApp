using Microsoft.AspNetCore.SignalR;
using WebMatcha.Services;
using WebMatcha.Models;

namespace WebMatcha.Hubs;

public class ChatHub : Hub
{
    private readonly MessageService _messageService;
    private readonly UserService _userService;
    private static readonly Dictionary<int, string> _userConnections = new();
    
    public ChatHub(MessageService messageService, UserService userService)
    {
        _messageService = messageService;
        _userService = userService;
    }
    
    public override async Task OnConnectedAsync()
    {
        // Get user ID from context (will be from auth later)
        var userId = Context.GetHttpContext()?.Request.Query["userId"].FirstOrDefault();
        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var id))
        {
            _userConnections[id] = Context.ConnectionId;
            await _userService.UpdateLastSeenAsync(id);
            
            // Notify all connected users about online status
            await Clients.Others.SendAsync("UserStatusChanged", new
            {
                userId = id,
                isOnline = true
            });
        }
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userEntry = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (userEntry.Key != 0)
        {
            _userConnections.Remove(userEntry.Key);
            await _userService.SetOfflineAsync(userEntry.Key);
            
            // Notify all connected users about offline status
            await Clients.Others.SendAsync("UserStatusChanged", new
            {
                userId = userEntry.Key,
                isOnline = false
            });
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessage(int senderId, int receiverId, string message)
    {
        // Save message to database
        var msg = await _messageService.SendMessageAsync(senderId, receiverId, message);
        
        if (msg != null)
        {
            // Send to receiver if online
            if (_userConnections.TryGetValue(receiverId, out var connectionId))
            {
                var sender = await _userService.GetUserByIdAsync(senderId);
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                {
                    id = msg.Id,
                    senderId = msg.SenderId,
                    senderName = sender?.FirstName ?? "Unknown",
                    senderPhoto = sender?.ProfilePhotoUrl,
                    content = msg.Content,
                    sentAt = msg.SentAt
                });
            }
            
            // Send confirmation to sender
            await Clients.Caller.SendAsync("MessageSent", new
            {
                id = msg.Id,
                receiverId = msg.ReceiverId,
                content = msg.Content,
                sentAt = msg.SentAt
            });
        }
    }
    
    public async Task MarkMessagesAsRead(int userId, int otherUserId)
    {
        await _messageService.MarkMessagesAsReadAsync(userId, otherUserId);
        
        // Notify the other user that messages were read
        if (_userConnections.TryGetValue(otherUserId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("MessagesRead", userId);
        }
    }
    
    public async Task JoinChat(int userId)
    {
        _userConnections[userId] = Context.ConnectionId;
        await _userService.UpdateLastSeenAsync(userId);
        
        // Notify all other users about online status
        await Clients.Others.SendAsync("UserStatusChanged", new
        {
            userId = userId,
            isOnline = true
        });
    }
    
    public async Task LeaveChat(int userId)
    {
        if (_userConnections.ContainsKey(userId))
        {
            _userConnections.Remove(userId);
            await _userService.SetOfflineAsync(userId);
            
            // Notify all other users about offline status
            await Clients.Others.SendAsync("UserStatusChanged", new
            {
                userId = userId,
                isOnline = false
            });
        }
    }
    
    public Task<bool> IsUserOnline(int userId)
    {
        return Task.FromResult(_userConnections.ContainsKey(userId));
    }
    
    public async Task SendTypingIndicator(int senderId, int receiverId, bool isTyping)
    {
        // Send typing indicator to receiver if online
        if (_userConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("UserTyping", new
            {
                userId = senderId,
                isTyping = isTyping
            });
        }
    }
}