using Npgsql;
using Dapper;
using WebMatcha.Models;

namespace WebMatcha.Services;

/// <summary>
/// NotificationService - Refactoré avec requêtes SQL manuelles
/// </summary>
public class NotificationService
{
    private readonly string _connectionString;

    public NotificationService(IConfiguration configuration)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
    }

    public async Task<Notification> CreateNotificationAsync(int userId, string type, string message)
    {
        const string sql = @"
            INSERT INTO notifications (user_id, type, message, is_read, created_at)
            VALUES (@UserId, @Type, @Message, false, @CreatedAt)
            RETURNING id, user_id AS UserId, type, message, is_read AS IsRead, created_at AS CreatedAt
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var notification = await connection.QueryFirstAsync<Notification>(sql, new
        {
            UserId = userId,
            Type = type,
            Message = message,
            CreatedAt = DateTime.UtcNow
        });

        return notification;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var sql = @"
            SELECT
                id, user_id AS UserId, type, message, is_read AS IsRead, created_at AS CreatedAt
            FROM notifications
            WHERE user_id = @UserId
        ";

        if (unreadOnly)
        {
            sql += " AND is_read = false";
        }

        sql += " ORDER BY created_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var notifications = await connection.QueryAsync<Notification>(sql, new { UserId = userId });
        return notifications.ToList();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        const string sql = "SELECT COUNT(*) FROM notifications WHERE user_id = @UserId AND is_read = false";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        const string sql = "UPDATE notifications SET is_read = true WHERE id = @NotificationId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { NotificationId = notificationId });
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        const string sql = "UPDATE notifications SET is_read = true WHERE user_id = @UserId AND is_read = false";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task DeleteNotificationAsync(int notificationId)
    {
        const string sql = "DELETE FROM notifications WHERE id = @NotificationId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { NotificationId = notificationId });
    }

    public async Task DeleteAllNotificationsAsync(int userId)
    {
        const string sql = "DELETE FROM notifications WHERE user_id = @UserId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task DeleteOldNotificationsAsync(int daysToKeep = 30)
    {
        const string sql = "DELETE FROM notifications WHERE created_at < @CutoffDate";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
    }
}
