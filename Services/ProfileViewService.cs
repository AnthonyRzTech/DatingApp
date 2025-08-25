using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;

namespace WebMatcha.Services;

public interface IProfileViewService
{
    Task RecordViewAsync(int viewerId, int viewedId);
    Task<List<ProfileView>> GetProfileViewsAsync(int userId);
    Task UpdateFameRatingAsync(int userId);
}

public class ProfileViewService : IProfileViewService
{
    private readonly MatchaDbContext _context;
    private readonly NotificationService _notificationService;
    
    public ProfileViewService(MatchaDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }
    
    public async Task RecordViewAsync(int viewerId, int viewedId)
    {
        if (viewerId == viewedId) return;
        
        // Check if already viewed recently (within 24 hours)
        var recentView = await _context.ProfileViews
            .FirstOrDefaultAsync(v => v.ViewerId == viewerId && 
                                     v.ViewedId == viewedId &&
                                     v.ViewedAt > DateTime.UtcNow.AddHours(-24));
                                     
        if (recentView != null) return;
        
        var view = new ProfileView
        {
            ViewerId = viewerId,
            ViewedId = viewedId,
            ViewedAt = DateTime.UtcNow
        };
        
        _context.ProfileViews.Add(view);
        await _context.SaveChangesAsync();
        
        // Send notification
        await _notificationService.CreateNotificationAsync(
            viewedId, 
            "view",
            "Someone viewed your profile");
            
        // Update fame rating
        await UpdateFameRatingAsync(viewedId);
    }
    
    public async Task<List<ProfileView>> GetProfileViewsAsync(int userId)
    {
        return await _context.ProfileViews
            .Where(v => v.ViewedId == userId)
            .OrderByDescending(v => v.ViewedAt)
            .Take(50)
            .ToListAsync();
    }
    
    public async Task UpdateFameRatingAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;
        
        // Calculate fame based on various factors
        var viewCount = await _context.ProfileViews
            .Where(v => v.ViewedId == userId)
            .CountAsync();
            
        var likeCount = await _context.Likes
            .Where(l => l.LikedId == userId)
            .CountAsync();
            
        var matchCount = await _context.Matches
            .Where(m => m.User1Id == userId || m.User2Id == userId)
            .CountAsync();
            
        var hasProfilePhoto = user.ProfilePhotoUrl != "/images/default-avatar.png";
        var hasPhotos = user.PhotoUrls.Any();
        var hasBio = !string.IsNullOrWhiteSpace(user.Biography) && user.Biography.Length > 50;
        var hasTags = user.InterestTags.Count >= 3;
        
        // Fame calculation algorithm
        int fame = 0;
        
        // Profile completeness (up to 20 points)
        if (hasProfilePhoto) fame += 5;
        if (hasPhotos) fame += 5;
        if (hasBio) fame += 5;
        if (hasTags) fame += 5;
        
        // Popularity (up to 80 points)
        fame += Math.Min(viewCount / 10, 20);  // Up to 20 points for views
        fame += Math.Min(likeCount * 2, 30);   // Up to 30 points for likes
        fame += Math.Min(matchCount * 5, 30);  // Up to 30 points for matches
        
        // Ensure fame is between 0 and 100
        fame = Math.Max(0, Math.Min(100, fame));
        
        user.FameRating = fame;
        await _context.SaveChangesAsync();
    }
}