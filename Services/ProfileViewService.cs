using Npgsql;
using Dapper;
using WebMatcha.Models;

namespace WebMatcha.Services;

public interface IProfileViewService
{
    Task RecordViewAsync(int viewerId, int viewedId);
    Task<List<ProfileView>> GetProfileViewsAsync(int userId);
    Task UpdateFameRatingAsync(int userId);
}

/// <summary>
/// ProfileViewService - Refactoré avec requêtes SQL manuelles
/// </summary>
public class ProfileViewService : IProfileViewService
{
    private readonly string _connectionString;
    private readonly NotificationService _notificationService;

    public ProfileViewService(IConfiguration configuration, NotificationService notificationService)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
        _notificationService = notificationService;
    }

    public async Task RecordViewAsync(int viewerId, int viewedId)
    {
        if (viewerId == viewedId) return;

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check if already viewed recently (within 24 hours)
        const string checkRecentSql = @"
            SELECT COUNT(*)
            FROM profile_views
            WHERE viewer_id = @ViewerId AND viewed_id = @ViewedId AND viewed_at > @CutoffTime
        ";
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var recentViewCount = await connection.ExecuteScalarAsync<int>(checkRecentSql, new { ViewerId = viewerId, ViewedId = viewedId, CutoffTime = cutoffTime });

        if (recentViewCount > 0) return;

        // Record the view
        const string insertSql = @"
            INSERT INTO profile_views (viewer_id, viewed_id, viewed_at)
            VALUES (@ViewerId, @ViewedId, @ViewedAt)
        ";
        await connection.ExecuteAsync(insertSql, new { ViewerId = viewerId, ViewedId = viewedId, ViewedAt = DateTime.UtcNow });

        // Send notification
        await _notificationService.CreateNotificationAsync(viewedId, "view", "Someone viewed your profile");

        // Update fame rating
        await UpdateFameRatingAsync(viewedId);
    }

    public async Task<List<ProfileView>> GetProfileViewsAsync(int userId)
    {
        const string sql = @"
            SELECT id, viewer_id AS ViewerId, viewed_id AS ViewedId, viewed_at AS ViewedAt
            FROM profile_views
            WHERE viewed_id = @UserId
            ORDER BY viewed_at DESC
            LIMIT 50
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var views = await connection.QueryAsync<ProfileView>(sql, new { UserId = userId });
        return views.ToList();
    }

    public async Task UpdateFameRatingAsync(int userId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Calculate fame in one SQL query
        const string sql = @"
            WITH user_stats AS (
                SELECT
                    (SELECT COUNT(*) FROM profile_views WHERE viewed_id = @UserId) AS view_count,
                    (SELECT COUNT(*) FROM likes WHERE liked_id = @UserId) AS like_count,
                    (SELECT COUNT(*) FROM matches WHERE user1id = @UserId OR user2id = @UserId) AS match_count
            ),
            user_profile AS (
                SELECT
                    CASE WHEN profile_photo_url != '/images/default-avatar.png' THEN 5 ELSE 0 END AS photo_points,
                    CASE WHEN LENGTH(biography) > 50 THEN 5 ELSE 0 END AS bio_points
                FROM users
                WHERE id = @UserId
            )
            SELECT
                LEAST(100, GREATEST(0,
                    up.photo_points + up.bio_points +
                    LEAST(us.view_count / 10, 20) +
                    LEAST(us.like_count * 2, 30) +
                    LEAST(us.match_count * 5, 30)
                )) AS fame_rating
            FROM user_stats us, user_profile up
        ";

        var fameRating = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });

        // Update user fame rating
        const string updateSql = "UPDATE users SET fame_rating = @FameRating WHERE id = @UserId";
        await connection.ExecuteAsync(updateSql, new { UserId = userId, FameRating = fameRating });
    }
}
