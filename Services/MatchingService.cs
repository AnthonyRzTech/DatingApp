using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;

namespace WebMatcha.Services;

public class MatchingService
{
    private readonly MatchaDbContext _context;
    private readonly NotificationService _notificationService;
    
    public MatchingService(MatchaDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }
    
    public async Task<bool> LikeUserAsync(int likerId, int likedId)
    {
        if (likerId == likedId) return false;
        
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.LikerId == likerId && l.LikedId == likedId);
        
        if (existingLike != null) return false;
        
        var like = new Like
        {
            LikerId = likerId,
            LikedId = likedId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Likes.Add(like);
        await _context.SaveChangesAsync();
        
        var reciprocalLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.LikerId == likedId && l.LikedId == likerId);
        
        if (reciprocalLike != null)
        {
            await CreateMatchAsync(likerId, likedId);
            
            var liker = await _context.Users.FindAsync(likerId);
            var liked = await _context.Users.FindAsync(likedId);
            
            if (liker != null && liked != null)
            {
                await _notificationService.CreateNotificationAsync(
                    likedId,
                    "match",
                    $"You matched with {liker.Username}!"
                );
                
                await _notificationService.CreateNotificationAsync(
                    likerId,
                    "match",
                    $"You matched with {liked.Username}!"
                );
            }
        }
        else
        {
            var liker = await _context.Users.FindAsync(likerId);
            if (liker != null)
            {
                await _notificationService.CreateNotificationAsync(
                    likedId,
                    "like",
                    $"{liker.Username} liked your profile"
                );
            }
        }
        
        return true;
    }
    
    public async Task<bool> UnlikeUserAsync(int likerId, int likedId)
    {
        var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.LikerId == likerId && l.LikedId == likedId);
        
        if (like == null) return false;
        
        var match = await _context.Matches
            .FirstOrDefaultAsync(m => 
                (m.User1Id == likerId && m.User2Id == likedId) ||
                (m.User1Id == likedId && m.User2Id == likerId));
        
        if (match != null)
        {
            _context.Matches.Remove(match);
        }
        
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
        
        var liker = await _context.Users.FindAsync(likerId);
        if (liker != null)
        {
            await _notificationService.CreateNotificationAsync(
                likedId,
                "unlike",
                $"{liker.Username} unliked your profile"
            );
        }
        
        return true;
    }
    
    public async Task<bool> IsLikedAsync(int likerId, int likedId)
    {
        return await _context.Likes
            .AnyAsync(l => l.LikerId == likerId && l.LikedId == likedId);
    }
    
    public async Task<bool> IsMatchedAsync(int user1Id, int user2Id)
    {
        return await _context.Matches
            .AnyAsync(m => 
                (m.User1Id == user1Id && m.User2Id == user2Id) ||
                (m.User1Id == user2Id && m.User2Id == user1Id));
    }
    
    public async Task<List<User>> GetUserLikesAsync(int userId)
    {
        var likedUserIds = await _context.Likes
            .Where(l => l.LikerId == userId)
            .Select(l => l.LikedId)
            .ToListAsync();
        
        return await _context.Users
            .Where(u => likedUserIds.Contains(u.Id))
            .ToListAsync();
    }
    
    public async Task<List<User>> GetUserLikedByAsync(int userId)
    {
        var likerUserIds = await _context.Likes
            .Where(l => l.LikedId == userId)
            .Select(l => l.LikerId)
            .ToListAsync();
        
        return await _context.Users
            .Where(u => likerUserIds.Contains(u.Id))
            .ToListAsync();
    }
    
    public async Task<List<User>> GetUserMatchesAsync(int userId)
    {
        var matchUserIds = await _context.Matches
            .Where(m => m.User1Id == userId || m.User2Id == userId)
            .Select(m => m.User1Id == userId ? m.User2Id : m.User1Id)
            .ToListAsync();
        
        return await _context.Users
            .Where(u => matchUserIds.Contains(u.Id))
            .ToListAsync();
    }
    
    public async Task BlockUserAsync(int blockerId, int blockedId)
    {
        if (blockerId == blockedId) return;
        
        var existingBlock = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
        
        if (existingBlock != null) return;
        
        var block = new Block
        {
            BlockerId = blockerId,
            BlockedId = blockedId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Blocks.Add(block);
        
        var like1 = await _context.Likes
            .FirstOrDefaultAsync(l => l.LikerId == blockerId && l.LikedId == blockedId);
        if (like1 != null) _context.Likes.Remove(like1);
        
        var like2 = await _context.Likes
            .FirstOrDefaultAsync(l => l.LikerId == blockedId && l.LikedId == blockerId);
        if (like2 != null) _context.Likes.Remove(like2);
        
        var match = await _context.Matches
            .FirstOrDefaultAsync(m => 
                (m.User1Id == blockerId && m.User2Id == blockedId) ||
                (m.User1Id == blockedId && m.User2Id == blockerId));
        if (match != null) _context.Matches.Remove(match);
        
        await _context.SaveChangesAsync();
    }
    
    public async Task UnblockUserAsync(int blockerId, int blockedId)
    {
        var block = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
        
        if (block != null)
        {
            _context.Blocks.Remove(block);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> IsBlockedAsync(int user1Id, int user2Id)
    {
        return await _context.Blocks
            .AnyAsync(b => 
                (b.BlockerId == user1Id && b.BlockedId == user2Id) ||
                (b.BlockerId == user2Id && b.BlockedId == user1Id));
    }
    
    public async Task<List<User>> GetBlockedUsersAsync(int userId)
    {
        var blockedUserIds = await _context.Blocks
            .Where(b => b.BlockerId == userId)
            .Select(b => b.BlockedId)
            .ToListAsync();
        
        return await _context.Users
            .Where(u => blockedUserIds.Contains(u.Id))
            .ToListAsync();
    }
    
    public async Task ReportUserAsync(int reporterId, int reportedId, string reason)
    {
        if (reporterId == reportedId) return;
        
        var report = new Report
        {
            ReporterId = reporterId,
            ReportedId = reportedId,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
    }
    
    public async Task RecordProfileViewAsync(int viewerId, int viewedId)
    {
        if (viewerId == viewedId) return;
        
        var isBlocked = await IsBlockedAsync(viewerId, viewedId);
        if (isBlocked) return;
        
        var existingView = await _context.ProfileViews
            .Where(v => v.ViewerId == viewerId && v.ViewedId == viewedId)
            .OrderByDescending(v => v.ViewedAt)
            .FirstOrDefaultAsync();
        
        if (existingView == null || (DateTime.UtcNow - existingView.ViewedAt).TotalHours > 1)
        {
            var view = new ProfileView
            {
                ViewerId = viewerId,
                ViewedId = viewedId,
                ViewedAt = DateTime.UtcNow
            };
            
            _context.ProfileViews.Add(view);
            await _context.SaveChangesAsync();
            
            var viewer = await _context.Users.FindAsync(viewerId);
            if (viewer != null)
            {
                await _notificationService.CreateNotificationAsync(
                    viewedId,
                    "view",
                    $"{viewer.Username} viewed your profile"
                );
            }
        }
    }
    
    public async Task<List<User>> GetProfileViewersAsync(int userId)
    {
        var viewerIds = await _context.ProfileViews
            .Where(v => v.ViewedId == userId)
            .OrderByDescending(v => v.ViewedAt)
            .Select(v => v.ViewerId)
            .Distinct()
            .ToListAsync();
        
        return await _context.Users
            .Where(u => viewerIds.Contains(u.Id))
            .ToListAsync();
    }
    
    private async Task CreateMatchAsync(int user1Id, int user2Id)
    {
        var existingMatch = await _context.Matches
            .FirstOrDefaultAsync(m => 
                (m.User1Id == user1Id && m.User2Id == user2Id) ||
                (m.User1Id == user2Id && m.User2Id == user1Id));
        
        if (existingMatch == null)
        {
            var match = new Match
            {
                User1Id = Math.Min(user1Id, user2Id),
                User2Id = Math.Max(user1Id, user2Id),
                MatchedAt = DateTime.UtcNow
            };
            
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
        }
    }
}