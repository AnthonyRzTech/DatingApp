using Npgsql;
using Dapper;
using WebMatcha.Models;

namespace WebMatcha.Services;

/// <summary>
/// MatchingService - Refactoré avec requêtes SQL manuelles et transactions
/// </summary>
public class MatchingService
{
    private readonly string _connectionString;
    private readonly NotificationService _notificationService;

    public MatchingService(IConfiguration configuration, NotificationService notificationService)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
        _notificationService = notificationService;
    }

    public async Task<bool> LikeUserAsync(int likerId, int likedId)
    {
        if (likerId == likedId) return false;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Check if like already exists
            const string checkLikeSql = "SELECT COUNT(*) FROM likes WHERE liker_id = @LikerId AND liked_id = @LikedId";
            var existingCount = await connection.ExecuteScalarAsync<int>(checkLikeSql, new { LikerId = likerId, LikedId = likedId }, transaction);

            if (existingCount > 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Insert the like
            const string insertLikeSql = "INSERT INTO likes (liker_id, liked_id, created_at) VALUES (@LikerId, @LikedId, @CreatedAt)";
            await connection.ExecuteAsync(insertLikeSql, new { LikerId = likerId, LikedId = likedId, CreatedAt = DateTime.UtcNow }, transaction);

            // Check for reciprocal like (match)
            const string checkReciprocalSql = "SELECT COUNT(*) FROM likes WHERE liker_id = @LikedId AND liked_id = @LikerId";
            var hasReciprocalLike = await connection.ExecuteScalarAsync<int>(checkReciprocalSql, new { LikerId = likerId, LikedId = likedId }, transaction) > 0;

            if (hasReciprocalLike)
            {
                // Create match
                const string createMatchSql = @"
                    INSERT INTO matches (user1id, user2id, matched_at)
                    VALUES (@User1Id, @User2Id, @MatchedAt)
                    ON CONFLICT (user1id, user2id) DO NOTHING
                ";
                var user1Id = Math.Min(likerId, likedId);
                var user2Id = Math.Max(likerId, likedId);
                await connection.ExecuteAsync(createMatchSql, new { User1Id = user1Id, User2Id = user2Id, MatchedAt = DateTime.UtcNow }, transaction);

                // Get usernames for notifications
                const string getUsernamesSql = "SELECT username FROM users WHERE id IN (@Id1, @Id2)";
                var usernames = (await connection.QueryAsync<string>(getUsernamesSql, new { Id1 = likerId, Id2 = likedId }, transaction)).ToList();

                await transaction.CommitAsync();

                // Send match notifications
                if (usernames.Count == 2)
                {
                    await _notificationService.CreateNotificationAsync(likedId, "match", $"You matched with {usernames[0]}!");
                    await _notificationService.CreateNotificationAsync(likerId, "match", $"You matched with {usernames[1]}!");
                }
            }
            else
            {
                // Get liker username for notification
                const string getLikerUsernameSql = "SELECT username FROM users WHERE id = @LikerId";
                var likerUsername = await connection.ExecuteScalarAsync<string>(getLikerUsernameSql, new { LikerId = likerId }, transaction);

                await transaction.CommitAsync();

                // Send like notification
                if (!string.IsNullOrEmpty(likerUsername))
                {
                    await _notificationService.CreateNotificationAsync(likedId, "like", $"{likerUsername} liked your profile");
                }
            }

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UnlikeUserAsync(int likerId, int likedId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Check if like exists
            const string checkLikeSql = "SELECT COUNT(*) FROM likes WHERE liker_id = @LikerId AND liked_id = @LikedId";
            var likeExists = await connection.ExecuteScalarAsync<int>(checkLikeSql, new { LikerId = likerId, LikedId = likedId }, transaction) > 0;

            if (!likeExists)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Delete match if exists
            const string deleteMatchSql = @"
                DELETE FROM matches
                WHERE (user1id = @User1Id AND user2id = @User2Id) OR (user1id = @User2Id AND user2id = @User1Id)
            ";
            var user1Id = Math.Min(likerId, likedId);
            var user2Id = Math.Max(likerId, likedId);
            await connection.ExecuteAsync(deleteMatchSql, new { User1Id = user1Id, User2Id = user2Id }, transaction);

            // Delete the like
            const string deleteLikeSql = "DELETE FROM likes WHERE liker_id = @LikerId AND liked_id = @LikedId";
            await connection.ExecuteAsync(deleteLikeSql, new { LikerId = likerId, LikedId = likedId }, transaction);

            // Get username for notification
            const string getUsernameSql = "SELECT username FROM users WHERE id = @LikerId";
            var likerUsername = await connection.ExecuteScalarAsync<string>(getUsernameSql, new { LikerId = likerId }, transaction);

            await transaction.CommitAsync();

            // Send unlike notification
            if (!string.IsNullOrEmpty(likerUsername))
            {
                await _notificationService.CreateNotificationAsync(likedId, "unlike", $"{likerUsername} unliked your profile");
            }

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> IsLikedAsync(int likerId, int likedId)
    {
        const string sql = "SELECT COUNT(*) FROM likes WHERE liker_id = @LikerId AND liked_id = @LikedId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var count = await connection.ExecuteScalarAsync<int>(sql, new { LikerId = likerId, LikedId = likedId });
        return count > 0;
    }

    public async Task<bool> IsMatchedAsync(int user1Id, int user2Id)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM matches
            WHERE (user1id = @User1Id AND user2id = @User2Id) OR (user1id = @User2Id AND user2id = @User1Id)
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var count = await connection.ExecuteScalarAsync<int>(sql, new { User1Id = user1Id, User2Id = user2Id });
        return count > 0;
    }

    public async Task<List<User>> GetUserLikesAsync(int userId)
    {
        const string sql = @"
            SELECT DISTINCT
                u.id, u.username, u.email, u.first_name AS FirstName, u.last_name AS LastName,
                u.birth_date AS BirthDate, u.gender, u.sexual_preference AS SexualPreference,
                u.biography, u.profile_photo_url AS ProfilePhotoUrl,
                u.latitude, u.longitude, u.fame_rating AS FameRating,
                u.is_online AS IsOnline, u.last_seen AS LastSeen, u.is_email_verified AS IsEmailVerified,
                u.email_verified_at AS EmailVerifiedAt, u.is_active AS IsActive,
                u.deactivated_at AS DeactivatedAt, u.created_at AS CreatedAt,
                l.created_at AS LikedAt
            FROM users u
            INNER JOIN likes l ON l.liked_id = u.id
            WHERE l.liker_id = @UserId
            ORDER BY l.created_at DESC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var users = await connection.QueryAsync<User>(sql, new { UserId = userId });
        return users.ToList();
    }

    public async Task<List<User>> GetUserLikedByAsync(int userId)
    {
        const string sql = @"
            SELECT DISTINCT
                u.id, u.username, u.email, u.first_name AS FirstName, u.last_name AS LastName,
                u.birth_date AS BirthDate, u.gender, u.sexual_preference AS SexualPreference,
                u.biography, u.profile_photo_url AS ProfilePhotoUrl,
                u.latitude, u.longitude, u.fame_rating AS FameRating,
                u.is_online AS IsOnline, u.last_seen AS LastSeen, u.is_email_verified AS IsEmailVerified,
                u.email_verified_at AS EmailVerifiedAt, u.is_active AS IsActive,
                u.deactivated_at AS DeactivatedAt, u.created_at AS CreatedAt,
                l.created_at AS LikedAt
            FROM users u
            INNER JOIN likes l ON l.liker_id = u.id
            WHERE l.liked_id = @UserId
            ORDER BY l.created_at DESC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var users = await connection.QueryAsync<User>(sql, new { UserId = userId });
        return users.ToList();
    }

    public async Task<List<User>> GetUserMatchesAsync(int userId)
    {
        const string sql = @"
            SELECT DISTINCT
                u.id, u.username, u.email, u.first_name AS FirstName, u.last_name AS LastName,
                u.birth_date AS BirthDate, u.gender, u.sexual_preference AS SexualPreference,
                u.biography, u.profile_photo_url AS ProfilePhotoUrl,
                u.latitude, u.longitude, u.fame_rating AS FameRating,
                u.is_online AS IsOnline, u.last_seen AS LastSeen, u.is_email_verified AS IsEmailVerified,
                u.email_verified_at AS EmailVerifiedAt, u.is_active AS IsActive,
                u.deactivated_at AS DeactivatedAt, u.created_at AS CreatedAt,
                m.matched_at AS MatchedAt
            FROM users u
            INNER JOIN matches m ON (
                (m.user1id = @UserId AND m.user2id = u.id) OR
                (m.user2id = @UserId AND m.user1id = u.id)
            )
            ORDER BY m.matched_at DESC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var users = await connection.QueryAsync<User>(sql, new { UserId = userId });
        return users.ToList();
    }

    public async Task BlockUserAsync(int blockerId, int blockedId)
    {
        if (blockerId == blockedId) return;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Check if already blocked
            const string checkBlockSql = "SELECT COUNT(*) FROM blocks WHERE blocker_id = @BlockerId AND blocked_id = @BlockedId";
            var alreadyBlocked = await connection.ExecuteScalarAsync<int>(checkBlockSql, new { BlockerId = blockerId, BlockedId = blockedId }, transaction) > 0;

            if (alreadyBlocked)
            {
                await transaction.RollbackAsync();
                return;
            }

            // Insert block
            const string insertBlockSql = "INSERT INTO blocks (blocker_id, blocked_id, created_at) VALUES (@BlockerId, @BlockedId, @CreatedAt)";
            await connection.ExecuteAsync(insertBlockSql, new { BlockerId = blockerId, BlockedId = blockedId, CreatedAt = DateTime.UtcNow }, transaction);

            // Delete all likes between these users
            const string deleteLikesSql = @"
                DELETE FROM likes
                WHERE (liker_id = @BlockerId AND liked_id = @BlockedId) OR (liker_id = @BlockedId AND liked_id = @BlockerId)
            ";
            await connection.ExecuteAsync(deleteLikesSql, new { BlockerId = blockerId, BlockedId = blockedId }, transaction);

            // Delete match if exists
            const string deleteMatchSql = @"
                DELETE FROM matches
                WHERE (user1id = @User1Id AND user2id = @User2Id) OR (user1id = @User2Id AND user2id = @User1Id)
            ";
            var user1Id = Math.Min(blockerId, blockedId);
            var user2Id = Math.Max(blockerId, blockedId);
            await connection.ExecuteAsync(deleteMatchSql, new { User1Id = user1Id, User2Id = user2Id }, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UnblockUserAsync(int blockerId, int blockedId)
    {
        const string sql = "DELETE FROM blocks WHERE blocker_id = @BlockerId AND blocked_id = @BlockedId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { BlockerId = blockerId, BlockedId = blockedId });
    }

    public async Task<bool> IsBlockedAsync(int user1Id, int user2Id)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM blocks
            WHERE (blocker_id = @User1Id AND blocked_id = @User2Id) OR (blocker_id = @User2Id AND blocked_id = @User1Id)
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var count = await connection.ExecuteScalarAsync<int>(sql, new { User1Id = user1Id, User2Id = user2Id });
        return count > 0;
    }

    public async Task<List<User>> GetBlockedUsersAsync(int userId)
    {
        const string sql = @"
            SELECT DISTINCT
                u.id, u.username, u.email, u.first_name AS FirstName, u.last_name AS LastName,
                u.birth_date AS BirthDate, u.gender, u.sexual_preference AS SexualPreference,
                u.biography, u.profile_photo_url AS ProfilePhotoUrl,
                u.latitude, u.longitude, u.fame_rating AS FameRating,
                u.is_online AS IsOnline, u.last_seen AS LastSeen, u.is_email_verified AS IsEmailVerified,
                u.email_verified_at AS EmailVerifiedAt, u.is_active AS IsActive,
                u.deactivated_at AS DeactivatedAt, u.created_at AS CreatedAt,
                b.created_at AS BlockedAt
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

    public async Task ReportUserAsync(int reporterId, int reportedId, string reason)
    {
        if (reporterId == reportedId) return;

        const string sql = @"
            INSERT INTO reports (reporter_id, reported_id, reason, created_at, is_resolved)
            VALUES (@ReporterId, @ReportedId, @Reason, @CreatedAt, false)
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new
        {
            ReporterId = reporterId,
            ReportedId = reportedId,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task RecordProfileViewAsync(int viewerId, int viewedId)
    {
        if (viewerId == viewedId) return;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check if users are blocked
        const string checkBlockSql = @"
            SELECT COUNT(*)
            FROM blocks
            WHERE (blocker_id = @ViewerId AND blocked_id = @ViewedId) OR (blocker_id = @ViewedId AND blocked_id = @ViewerId)
        ";
        var isBlocked = await connection.ExecuteScalarAsync<int>(checkBlockSql, new { ViewerId = viewerId, ViewedId = viewedId }) > 0;

        if (isBlocked) return;

        // Check recent view (don't record if < 1 hour)
        const string checkRecentViewSql = @"
            SELECT viewed_at FROM profile_views
            WHERE viewer_id = @ViewerId AND viewed_id = @ViewedId
            ORDER BY viewed_at DESC
            LIMIT 1
        ";
        var lastView = await connection.QueryFirstOrDefaultAsync<DateTime?>(checkRecentViewSql, new { ViewerId = viewerId, ViewedId = viewedId });

        if (lastView.HasValue && (DateTime.UtcNow - lastView.Value).TotalHours < 1)
        {
            return;
        }

        // Record the view
        const string insertViewSql = "INSERT INTO profile_views (viewer_id, viewed_id, viewed_at) VALUES (@ViewerId, @ViewedId, @ViewedAt)";
        await connection.ExecuteAsync(insertViewSql, new { ViewerId = viewerId, ViewedId = viewedId, ViewedAt = DateTime.UtcNow });

        // Get viewer username for notification
        const string getUsernameSql = "SELECT username FROM users WHERE id = @ViewerId";
        var viewerUsername = await connection.ExecuteScalarAsync<string>(getUsernameSql, new { ViewerId = viewerId });

        // Send notification
        if (!string.IsNullOrEmpty(viewerUsername))
        {
            await _notificationService.CreateNotificationAsync(viewedId, "view", $"{viewerUsername} viewed your profile");
        }
    }

    public async Task<List<User>> GetProfileViewersAsync(int userId)
    {
        const string sql = @"
            SELECT DISTINCT ON (u.id)
                u.id, u.username, u.email, u.first_name AS FirstName, u.last_name AS LastName,
                u.birth_date AS BirthDate, u.gender, u.sexual_preference AS SexualPreference,
                u.biography, u.profile_photo_url AS ProfilePhotoUrl,
                u.latitude, u.longitude, u.fame_rating AS FameRating,
                u.is_online AS IsOnline, u.last_seen AS LastSeen, u.is_email_verified AS IsEmailVerified,
                u.email_verified_at AS EmailVerifiedAt, u.is_active AS IsActive,
                u.deactivated_at AS DeactivatedAt, u.created_at AS CreatedAt,
                pv.viewed_at AS ViewedAt
            FROM users u
            INNER JOIN profile_views pv ON pv.viewer_id = u.id
            WHERE pv.viewed_id = @UserId
            ORDER BY u.id, pv.viewed_at DESC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var users = await connection.QueryAsync<User>(sql, new { UserId = userId });
        return users.ToList();
    }
}
