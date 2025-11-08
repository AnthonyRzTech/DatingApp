using Npgsql;
using Dapper;
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

/// <summary>
/// BlockReportService - Refactoré avec requêtes SQL manuelles
/// </summary>
public class BlockReportService : IBlockReportService
{
    private readonly string _connectionString;

    public BlockReportService(IConfiguration configuration)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
    }

    public async Task<bool> BlockUserAsync(int blockerId, int blockedId)
    {
        if (blockerId == blockedId) return false;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Check if already blocked
            const string checkSql = "SELECT COUNT(*) FROM blocks WHERE blocker_id = @BlockerId AND blocked_id = @BlockedId";
            var existsCount = await connection.ExecuteScalarAsync<int>(checkSql, new { BlockerId = blockerId, BlockedId = blockedId }, transaction);

            if (existsCount > 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Insert block
            const string insertSql = "INSERT INTO blocks (blocker_id, blocked_id, created_at) VALUES (@BlockerId, @BlockedId, @CreatedAt)";
            await connection.ExecuteAsync(insertSql, new { BlockerId = blockerId, BlockedId = blockedId, CreatedAt = DateTime.UtcNow }, transaction);

            // Remove likes
            const string deleteLikesSql = @"
                DELETE FROM likes
                WHERE (liker_id = @BlockerId AND liked_id = @BlockedId) OR (liker_id = @BlockedId AND liked_id = @BlockerId)
            ";
            await connection.ExecuteAsync(deleteLikesSql, new { BlockerId = blockerId, BlockedId = blockedId }, transaction);

            // Remove matches
            const string deleteMatchesSql = @"
                DELETE FROM matches
                WHERE (user1id = @User1Id AND user2id = @User2Id) OR (user1id = @User2Id AND user2id = @User1Id)
            ";
            var user1Id = Math.Min(blockerId, blockedId);
            var user2Id = Math.Max(blockerId, blockedId);
            await connection.ExecuteAsync(deleteMatchesSql, new { User1Id = user1Id, User2Id = user2Id }, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> UnblockUserAsync(int blockerId, int blockedId)
    {
        const string sql = "DELETE FROM blocks WHERE blocker_id = @BlockerId AND blocked_id = @BlockedId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteAsync(sql, new { BlockerId = blockerId, BlockedId = blockedId });
        return rowsAffected > 0;
    }

    public async Task<bool> IsBlockedAsync(int userId1, int userId2)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM blocks
            WHERE (blocker_id = @UserId1 AND blocked_id = @UserId2) OR (blocker_id = @UserId2 AND blocked_id = @UserId1)
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId1 = userId1, UserId2 = userId2 });
        return count > 0;
    }

    public async Task<List<User>> GetBlockedUsersAsync(int userId)
    {
        const string sql = @"
            SELECT
                u.id, u.username, u.email, u.first_name AS FirstName, u.last_name AS LastName,
                u.birth_date AS BirthDate, u.gender, u.sexual_preference AS SexualPreference,
                u.biography, u.profile_photo_url AS ProfilePhotoUrl,
                u.latitude, u.longitude, u.fame_rating AS FameRating,
                u.is_online AS IsOnline, u.last_seen AS LastSeen, u.is_email_verified AS IsEmailVerified,
                u.email_verified_at AS EmailVerifiedAt, u.is_active AS IsActive,
                u.deactivated_at AS DeactivatedAt, u.created_at AS CreatedAt
            FROM users u
            INNER JOIN blocks b ON b.blocked_id = u.id
            WHERE b.blocker_id = @UserId
            ORDER BY b.created_at DESC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var users = await connection.QueryAsync<User>(sql, new { UserId = userId });
        return users.ToList();
    }

    public async Task<bool> ReportUserAsync(int reporterId, int reportedId, string reason)
    {
        if (reporterId == reportedId) return false;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO reports (reporter_id, reported_id, reason, created_at, is_resolved)
            VALUES (@ReporterId, @ReportedId, @Reason, @CreatedAt, false)
        ";
        await connection.ExecuteAsync(sql, new { ReporterId = reporterId, ReportedId = reportedId, Reason = reason, CreatedAt = DateTime.UtcNow });

        // Auto-block after report
        await BlockUserAsync(reporterId, reportedId);

        return true;
    }
}
