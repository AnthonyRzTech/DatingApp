using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;

namespace WebMatcha.Services;

public class NotificationService
{
    private readonly MatchaDbContext _context;
    
    public NotificationService(MatchaDbContext context)
    {
        _context = context;
    }
    
    public async Task<Notification> CreateNotificationAsync(int userId, string type, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        
        return notification;
    }
    
    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);
        
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }
        
        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
    
    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
        
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteNotificationAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task DeleteAllNotificationsAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();
        
        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteOldNotificationsAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        
        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate)
            .ToListAsync();
        
        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
    }
}