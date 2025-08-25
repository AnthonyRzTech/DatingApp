using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;

namespace WebMatcha.Services;

public interface IBlockReportService
{
    Task<bool> BlockUserAsync(int blockerId, int blockedId);
    Task<bool> UnblockUserAsync(int blockerId, int blockedId);
    Task<bool> IsBlockedAsync(int userId1, int userId2);
    Task<List<User>> GetBlockedUsersAsync(int userId);
    Task<bool> ReportUserAsync(int reporterId, int reportedId, string reason);
}

public class BlockReportService : IBlockReportService
{
    private readonly MatchaDbContext _context;
    
    public BlockReportService(MatchaDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> BlockUserAsync(int blockerId, int blockedId)
    {
        if (blockerId == blockedId) return false;
        
        // Check if already blocked
        var existing = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
            
        if (existing != null) return false;
        
        var block = new Block
        {
            BlockerId = blockerId,
            BlockedId = blockedId,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Blocks.Add(block);
        
        // Remove any existing likes/matches
        var likes = await _context.Likes
            .Where(l => (l.LikerId == blockerId && l.LikedId == blockedId) ||
                       (l.LikerId == blockedId && l.LikedId == blockerId))
            .ToListAsync();
            
        _context.Likes.RemoveRange(likes);
        
        var matches = await _context.Matches
            .Where(m => (m.User1Id == blockerId && m.User2Id == blockedId) ||
                       (m.User1Id == blockedId && m.User2Id == blockerId))
            .ToListAsync();
            
        _context.Matches.RemoveRange(matches);
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> UnblockUserAsync(int blockerId, int blockedId)
    {
        var block = await _context.Blocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
            
        if (block == null) return false;
        
        _context.Blocks.Remove(block);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> IsBlockedAsync(int userId1, int userId2)
    {
        return await _context.Blocks
            .AnyAsync(b => (b.BlockerId == userId1 && b.BlockedId == userId2) ||
                          (b.BlockerId == userId2 && b.BlockedId == userId1));
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
    
    public async Task<bool> ReportUserAsync(int reporterId, int reportedId, string reason)
    {
        if (reporterId == reportedId) return false;
        
        var report = new Report
        {
            ReporterId = reporterId,
            ReportedId = reportedId,
            Reason = reason,
            CreatedAt = DateTime.UtcNow,
            IsResolved = false
        };
        
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        
        // Auto-block after report
        await BlockUserAsync(reporterId, reportedId);
        
        return true;
    }
}