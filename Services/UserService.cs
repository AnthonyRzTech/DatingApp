using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;

namespace WebMatcha.Services;

public class UserService
{
    private readonly MatchaDbContext _context;
    
    public UserService(MatchaDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
    
    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<List<User>> GetUserSuggestionsAsync(int userId, int limit = 10)
    {
        var currentUser = await GetUserByIdAsync(userId);
        if (currentUser == null) return new List<User>();
        
        var blockedUserIds = await _context.Blocks
            .Where(b => b.BlockerId == userId || b.BlockedId == userId)
            .Select(b => b.BlockerId == userId ? b.BlockedId : b.BlockerId)
            .ToListAsync();
        
        var likedUserIds = await _context.Likes
            .Where(l => l.LikerId == userId)
            .Select(l => l.LikedId)
            .ToListAsync();
        
        var query = _context.Users
            .Where(u => u.Id != userId)
            .Where(u => !blockedUserIds.Contains(u.Id))
            .Where(u => !likedUserIds.Contains(u.Id));
        
        if (!string.IsNullOrEmpty(currentUser.SexualPreference) && currentUser.SexualPreference != "both")
        {
            query = query.Where(u => u.Gender == currentUser.SexualPreference);
        }
        
        var users = await query.Take(limit * 2).ToListAsync();
        
        foreach (var user in users)
        {
            user.Distance = CalculateDistance(
                currentUser.Latitude, currentUser.Longitude,
                user.Latitude, user.Longitude
            );
        }
        
        return users
            .OrderBy(u => u.Distance)
            .ThenByDescending(u => u.FameRating)
            .Take(limit)
            .ToList();
    }
    
    public async Task<List<User>> SearchUsersAsync(
        int userId,
        int? ageMin = null,
        int? ageMax = null,
        int? fameMin = null,
        int? fameMax = null,
        double? distanceMax = null,
        List<string>? tags = null)
    {
        var currentUser = await GetUserByIdAsync(userId);
        if (currentUser == null) return new List<User>();
        
        var blockedUserIds = await _context.Blocks
            .Where(b => b.BlockerId == userId || b.BlockedId == userId)
            .Select(b => b.BlockerId == userId ? b.BlockedId : b.BlockerId)
            .ToListAsync();
        
        var query = _context.Users
            .Where(u => u.Id != userId)
            .Where(u => !blockedUserIds.Contains(u.Id));
        
        if (fameMin.HasValue)
            query = query.Where(u => u.FameRating >= fameMin.Value);
        
        if (fameMax.HasValue)
            query = query.Where(u => u.FameRating <= fameMax.Value);
        
        var users = await query.ToListAsync();
        
        var currentYear = DateTime.Now.Year;
        users = users.Where(u =>
        {
            var age = currentYear - u.BirthDate.Year;
            if (ageMin.HasValue && age < ageMin.Value) return false;
            if (ageMax.HasValue && age > ageMax.Value) return false;
            
            u.Distance = CalculateDistance(
                currentUser.Latitude, currentUser.Longitude,
                u.Latitude, u.Longitude
            );
            
            if (distanceMax.HasValue && u.Distance > distanceMax.Value) return false;
            
            if (tags != null && tags.Any())
            {
                return tags.Any(tag => u.InterestTags.Contains(tag));
            }
            
            return true;
        }).ToList();
        
        return users.OrderBy(u => u.Distance).ToList();
    }
    
    public async Task UpdateLastSeenAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastSeen = DateTime.UtcNow;
            user.IsOnline = true;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task SetOfflineAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsOnline = false;
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task UpdateFameRatingAsync(int userId)
    {
        var likesCount = await _context.Likes.CountAsync(l => l.LikedId == userId);
        var viewsCount = await _context.ProfileViews.CountAsync(v => v.ViewedId == userId);
        var matchesCount = await _context.Matches
            .CountAsync(m => m.User1Id == userId || m.User2Id == userId);
        
        var fameRating = (likesCount * 10) + (viewsCount * 2) + (matchesCount * 20);
        fameRating = Math.Min(100, fameRating);
        
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.FameRating = fameRating;
            await _context.SaveChangesAsync();
        }
    }
    
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
    
    private double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}