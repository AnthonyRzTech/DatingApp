using Npgsql;
using Dapper;

namespace WebMatcha.Services;

/// <summary>
/// DatabaseSchemaService - Creates database schema using pure SQL (no EF Core)
/// </summary>
public class DatabaseSchemaService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseSchemaService> _logger;

    public DatabaseSchemaService(IConfiguration configuration, ILogger<DatabaseSchemaService> logger)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
        _logger = logger;
    }

    public async Task EnsureDatabaseSchemaAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            _logger.LogInformation("Ensuring database schema exists...");

            // Create tables
            await CreateUsersTableAsync(connection);
            await CreateLikesTableAsync(connection);
            await CreateMatchesTableAsync(connection);
            await CreateMessagesTableAsync(connection);
            await CreateNotificationsTableAsync(connection);
            await CreateProfileViewsTableAsync(connection);
            await CreateBlocksTableAsync(connection);
            await CreateReportsTableAsync(connection);
            await CreateUserPasswordsTableAsync(connection);
            await CreateEmailVerificationsTableAsync(connection);
            await CreatePasswordResetsTableAsync(connection);

            _logger.LogInformation("Database schema ensured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure database schema");
            throw;
        }
    }

    private async Task CreateUsersTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(50) NOT NULL UNIQUE,
                email VARCHAR(100) NOT NULL UNIQUE,
                first_name VARCHAR(50) NOT NULL,
                last_name VARCHAR(50) NOT NULL,
                birth_date TIMESTAMP WITH TIME ZONE NOT NULL,
                gender VARCHAR(20) NOT NULL,
                sexual_preference VARCHAR(20) NOT NULL,
                biography VARCHAR(500) NOT NULL DEFAULT '',
                interest_tags TEXT NOT NULL DEFAULT '',
                profile_photo_url TEXT NOT NULL DEFAULT '/images/default-avatar.png',
                photo_urls TEXT NOT NULL DEFAULT '',
                latitude DOUBLE PRECISION NOT NULL DEFAULT 0,
                longitude DOUBLE PRECISION NOT NULL DEFAULT 0,
                fame_rating INTEGER NOT NULL DEFAULT 0,
                is_online BOOLEAN NOT NULL DEFAULT FALSE,
                last_seen TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                is_email_verified BOOLEAN NOT NULL DEFAULT FALSE,
                email_verified_at TIMESTAMP WITH TIME ZONE,
                is_active BOOLEAN NOT NULL DEFAULT TRUE,
                deactivated_at TIMESTAMP WITH TIME ZONE,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateLikesTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS likes (
                id SERIAL PRIMARY KEY,
                liker_id INTEGER NOT NULL,
                liked_id INTEGER NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                UNIQUE(liker_id, liked_id)
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateMatchesTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS matches (
                id SERIAL PRIMARY KEY,
                user1id INTEGER NOT NULL,
                user2id INTEGER NOT NULL,
                matched_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                UNIQUE(user1id, user2id)
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateMessagesTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS messages (
                id SERIAL PRIMARY KEY,
                sender_id INTEGER NOT NULL,
                receiver_id INTEGER NOT NULL,
                content VARCHAR(1000) NOT NULL,
                sent_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                is_read BOOLEAN NOT NULL DEFAULT FALSE
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateNotificationsTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS notifications (
                id SERIAL PRIMARY KEY,
                user_id INTEGER NOT NULL,
                type TEXT NOT NULL,
                message TEXT NOT NULL,
                is_read BOOLEAN NOT NULL DEFAULT FALSE,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateProfileViewsTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS profile_views (
                id SERIAL PRIMARY KEY,
                viewer_id INTEGER NOT NULL,
                viewed_id INTEGER NOT NULL,
                viewed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateBlocksTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS blocks (
                id SERIAL PRIMARY KEY,
                blocker_id INTEGER NOT NULL,
                blocked_id INTEGER NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                UNIQUE(blocker_id, blocked_id)
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateReportsTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS reports (
                id SERIAL PRIMARY KEY,
                reporter_id INTEGER NOT NULL,
                reported_id INTEGER NOT NULL,
                reason TEXT NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                is_resolved BOOLEAN NOT NULL DEFAULT FALSE
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateUserPasswordsTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS user_passwords (
                id SERIAL PRIMARY KEY,
                user_id INTEGER NOT NULL UNIQUE,
                password_hash VARCHAR(255) NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreateEmailVerificationsTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS email_verifications (
                id SERIAL PRIMARY KEY,
                user_id INTEGER NOT NULL,
                token VARCHAR(255) NOT NULL UNIQUE,
                expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
                is_used BOOLEAN NOT NULL DEFAULT FALSE,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );
        ";
        await connection.ExecuteAsync(sql);
    }

    private async Task CreatePasswordResetsTableAsync(NpgsqlConnection connection)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS password_resets (
                id SERIAL PRIMARY KEY,
                user_id INTEGER NOT NULL,
                token VARCHAR(255) NOT NULL UNIQUE,
                expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
                is_used BOOLEAN NOT NULL DEFAULT FALSE,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );
        ";
        await connection.ExecuteAsync(sql);
    }
}
