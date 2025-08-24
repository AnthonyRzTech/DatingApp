using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;

namespace WebMatcha.Services;

public class MessageService
{
    private readonly MatchaDbContext _context;
    private readonly MatchingService _matchingService;
    
    public MessageService(MatchaDbContext context, MatchingService matchingService)
    {
        _context = context;
        _matchingService = matchingService;
    }
    
    public async Task<Message?> SendMessageAsync(int senderId, int receiverId, string content)
    {
        // Check if users are matched (can only message matches)
        var isMatched = await _matchingService.IsMatchedAsync(senderId, receiverId);
        if (!isMatched)
        {
            return null; // Can't send message if not matched
        }
        
        // Check if either user has blocked the other
        var isBlocked = await _matchingService.IsBlockedAsync(senderId, receiverId);
        if (isBlocked)
        {
            return null; // Can't send message if blocked
        }
        
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        
        return message;
    }
    
    public async Task<List<Message>> GetConversationAsync(int user1Id, int user2Id)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                       (m.SenderId == user2Id && m.ReceiverId == user1Id))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }
    
    public async Task<List<Message>> GetRecentMessagesAsync(int userId, int otherUserId, int count = 50)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                       (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderByDescending(m => m.SentAt)
            .Take(count)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }
    
    public async Task MarkMessagesAsReadAsync(int receiverId, int senderId)
    {
        var unreadMessages = await _context.Messages
            .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
            .ToListAsync();
        
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }
        
        await _context.SaveChangesAsync();
    }
    
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .CountAsync();
    }
    
    public async Task<Dictionary<int, int>> GetUnreadCountsByUserAsync(int userId)
    {
        var unreadCounts = await _context.Messages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .GroupBy(m => m.SenderId)
            .Select(g => new { SenderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SenderId, x => x.Count);
        
        return unreadCounts;
    }
    
    public async Task<List<ConversationSummary>> GetConversationsAsync(int userId)
    {
        // Get all messages for the user
        var messages = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
        
        // Group by the other user
        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => new ConversationSummary
            {
                OtherUserId = g.Key,
                LastMessage = g.First().Content,
                LastMessageTime = g.First().SentAt,
                UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
            })
            .ToList();
        
        // Get user details for each conversation
        foreach (var conv in conversations)
        {
            var user = await _context.Users.FindAsync(conv.OtherUserId);
            if (user != null)
            {
                conv.OtherUserName = $"{user.FirstName} {user.LastName}";
                conv.OtherUserPhoto = user.ProfilePhotoUrl;
                conv.IsOnline = user.IsOnline;
            }
        }
        
        return conversations.OrderByDescending(c => c.LastMessageTime).ToList();
    }
    
    public async Task<Message?> GetLastMessageAsync(int user1Id, int user2Id)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                       (m.SenderId == user2Id && m.ReceiverId == user1Id))
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefaultAsync();
    }
}

public class ConversationSummary
{
    public int OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserPhoto { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
}