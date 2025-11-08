using Npgsql;
using Dapper;

namespace WebMatcha.Services;

/// <summary>
/// DatabaseOptimizationService - Applique les optimisations et index PostgreSQL
/// </summary>
public class DatabaseOptimizationService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseOptimizationService> _logger;

    public DatabaseOptimizationService(IConfiguration configuration, ILogger<DatabaseOptimizationService> logger)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
        _logger = logger;
    }

    public async Task ApplyOptimizationsAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        try
        {
            _logger.LogInformation("Applying database indexes and optimizations...");

            // Index sur users table
            await ExecuteIndexAsync(connection, "idx_users_username", "users(username)");
            await ExecuteIndexAsync(connection, "idx_users_email", "users(email)");
            await ExecuteIndexAsync(connection, "idx_users_gender", "users(gender)");
            await ExecuteIndexAsync(connection, "idx_users_sexual_preference", "users(sexual_preference)");
            await ExecuteIndexAsync(connection, "idx_users_location", "users(latitude, longitude)");
            await ExecuteIndexAsync(connection, "idx_users_fame_rating", "users(fame_rating DESC)");
            await ExecuteIndexAsync(connection, "idx_users_birth_date", "users(birth_date)");
            await ExecuteIndexAsync(connection, "idx_users_is_online", "users(is_online)");
            await ExecuteIndexAsync(connection, "idx_users_is_active", "users(is_active)");

            // Index sur likes table
            await ExecuteIndexAsync(connection, "idx_likes_liker_id", "likes(liker_id)");
            await ExecuteIndexAsync(connection, "idx_likes_liked_id", "likes(liked_id)");
            await ExecuteIndexAsync(connection, "idx_likes_both", "likes(liker_id, liked_id)");

            // Index sur matches table
            await ExecuteIndexAsync(connection, "idx_matches_user1_id", "matches(user1id)");
            await ExecuteIndexAsync(connection, "idx_matches_user2_id", "matches(user2id)");
            await ExecuteIndexAsync(connection, "idx_matches_both", "matches(user1id, user2id)");

            // Index sur messages table
            await ExecuteIndexAsync(connection, "idx_messages_sender_id", "messages(sender_id)");
            await ExecuteIndexAsync(connection, "idx_messages_receiver_id", "messages(receiver_id)");
            await ExecuteIndexAsync(connection, "idx_messages_sent_at", "messages(sent_at DESC)");
            await ExecuteIndexAsync(connection, "idx_messages_is_read", "messages(is_read)");

            // Index sur notifications table
            await ExecuteIndexAsync(connection, "idx_notifications_user_id", "notifications(user_id)");
            await ExecuteIndexAsync(connection, "idx_notifications_is_read", "notifications(is_read)");
            await ExecuteIndexAsync(connection, "idx_notifications_user_unread", "notifications(user_id, is_read)");

            // Index sur profile_views table
            await ExecuteIndexAsync(connection, "idx_profile_views_viewer_id", "profile_views(viewer_id)");
            await ExecuteIndexAsync(connection, "idx_profile_views_viewed_id", "profile_views(viewed_id)");
            await ExecuteIndexAsync(connection, "idx_profile_views_both", "profile_views(viewer_id, viewed_id)");

            // Index sur blocks table
            await ExecuteIndexAsync(connection, "idx_blocks_blocker_id", "blocks(blocker_id)");
            await ExecuteIndexAsync(connection, "idx_blocks_blocked_id", "blocks(blocked_id)");
            await ExecuteIndexAsync(connection, "idx_blocks_both", "blocks(blocker_id, blocked_id)");

            // Index sur reports table
            await ExecuteIndexAsync(connection, "idx_reports_reporter_id", "reports(reporter_id)");
            await ExecuteIndexAsync(connection, "idx_reports_reported_id", "reports(reported_id)");

            // Index sur user_passwords table
            await ExecuteIndexAsync(connection, "idx_user_passwords_user_id", "user_passwords(user_id)");

            // Index sur email_verifications table
            await ExecuteIndexAsync(connection, "idx_email_verifications_token", "email_verifications(token)");
            await ExecuteIndexAsync(connection, "idx_email_verifications_user_id", "email_verifications(user_id)");

            // Index sur password_resets table
            await ExecuteIndexAsync(connection, "idx_password_resets_token", "password_resets(token)");
            await ExecuteIndexAsync(connection, "idx_password_resets_user_id", "password_resets(user_id)");

            // Index composites pour requêtes complexes
            await ExecuteIndexAsync(connection, "idx_users_search", "users(gender, sexual_preference, is_active, fame_rating DESC)");

            // Exécuter ANALYZE pour optimiser les statistiques
            await connection.ExecuteAsync("ANALYZE users");
            await connection.ExecuteAsync("ANALYZE likes");
            await connection.ExecuteAsync("ANALYZE matches");
            await connection.ExecuteAsync("ANALYZE messages");
            await connection.ExecuteAsync("ANALYZE notifications");
            await connection.ExecuteAsync("ANALYZE profile_views");

            _logger.LogInformation("Database optimizations applied successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying database optimizations: {Message}", ex.Message);
            throw;
        }
    }

    private async Task ExecuteIndexAsync(NpgsqlConnection connection, string indexName, string indexDefinition)
    {
        try
        {
            var sql = $"CREATE INDEX IF NOT EXISTS {indexName} ON {indexDefinition}";
            await connection.ExecuteAsync(sql);
            _logger.LogDebug("Index {IndexName} created/verified", indexName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create index {IndexName}: {Message}", indexName, ex.Message);
        }
    }
}
