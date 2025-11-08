using Npgsql;
using Dapper;
using WebMatcha.Models;

namespace WebMatcha.Services;

/// <summary>
/// UserService - Refactoré avec requêtes SQL manuelles (conforme au sujet)
/// </summary>
public class UserService
{
    private readonly string _connectionString;

    public UserService(IConfiguration configuration)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        const string sql = @"
            SELECT
                id, username, email, first_name AS FirstName, last_name AS LastName,
                birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
                biography, profile_photo_url AS ProfilePhotoUrl,
                latitude, longitude, fame_rating AS FameRating,
                is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
                email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
                deactivated_at AS DeactivatedAt, created_at AS CreatedAt
            FROM users
            WHERE id = @Id
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });

        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        const string sql = @"
            SELECT
                id, username, email, first_name AS FirstName, last_name AS LastName,
                birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
                biography, profile_photo_url AS ProfilePhotoUrl,
                latitude, longitude, fame_rating AS FameRating,
                is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
                email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
                deactivated_at AS DeactivatedAt, created_at AS CreatedAt
            FROM users
            WHERE username = @Username
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });

        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        const string sql = @"
            SELECT
                id, username, email, first_name AS FirstName, last_name AS LastName,
                birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
                biography, profile_photo_url AS ProfilePhotoUrl,
                latitude, longitude, fame_rating AS FameRating,
                is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
                email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
                deactivated_at AS DeactivatedAt, created_at AS CreatedAt
            FROM users
            WHERE email = @Email
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });

        return user;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        const string sql = @"
            SELECT
                id, username, email, first_name AS FirstName, last_name AS LastName,
                birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
                biography, profile_photo_url AS ProfilePhotoUrl,
                latitude, longitude, fame_rating AS FameRating,
                is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
                email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
                deactivated_at AS DeactivatedAt, created_at AS CreatedAt
            FROM users
            ORDER BY created_at DESC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var users = await connection.QueryAsync<User>(sql);

        return users.ToList();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        const string sql = @"
            INSERT INTO users (
                username, email, first_name, last_name, birth_date,
                gender, sexual_preference, biography, interest_tags,
                profile_photo_url, photo_urls, latitude, longitude,
                fame_rating, is_online, last_seen, is_email_verified,
                email_verified_at, is_active, deactivated_at, created_at
            ) VALUES (
                @Username, @Email, @FirstName, @LastName, @BirthDate,
                @Gender, @SexualPreference, @Biography, @InterestTags,
                @ProfilePhotoUrl, @PhotoUrls, @Latitude, @Longitude,
                @FameRating, @IsOnline, @LastSeen, @IsEmailVerified,
                @EmailVerifiedAt, @IsActive, @DeactivatedAt, @CreatedAt
            )
            RETURNING id
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new
        {
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.BirthDate,
            user.Gender,
            user.SexualPreference,
            user.Biography,
            InterestTags = string.Join(',', user.InterestTags),
            user.ProfilePhotoUrl,
            PhotoUrls = string.Join(',', user.PhotoUrls),
            user.Latitude,
            user.Longitude,
            user.FameRating,
            user.IsOnline,
            user.LastSeen,
            user.IsEmailVerified,
            user.EmailVerifiedAt,
            user.IsActive,
            user.DeactivatedAt,
            CreatedAt = DateTime.UtcNow
        };

        user.Id = await connection.ExecuteScalarAsync<int>(sql, parameters);
        user.CreatedAt = parameters.CreatedAt;

        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        // Sanitize user inputs to prevent XSS attacks (CRITICAL security requirement)
        user.Biography = InputSanitizer.SanitizeBiography(user.Biography);
        user.InterestTags = user.InterestTags.Select(tag => InputSanitizer.SanitizeText(tag)).ToList();
        user.FirstName = InputSanitizer.SanitizeText(user.FirstName);
        user.LastName = InputSanitizer.SanitizeText(user.LastName);

        const string sql = @"
            UPDATE users SET
                username = @Username,
                email = @Email,
                first_name = @FirstName,
                last_name = @LastName,
                birth_date = @BirthDate,
                gender = @Gender,
                sexual_preference = @SexualPreference,
                biography = @Biography,
                interest_tags = @InterestTags,
                profile_photo_url = @ProfilePhotoUrl,
                photo_urls = @PhotoUrls,
                latitude = @Latitude,
                longitude = @Longitude,
                fame_rating = @FameRating,
                is_online = @IsOnline,
                last_seen = @LastSeen,
                is_email_verified = @IsEmailVerified,
                email_verified_at = @EmailVerifiedAt,
                is_active = @IsActive,
                deactivated_at = @DeactivatedAt
            WHERE id = @Id
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new
        {
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.BirthDate,
            user.Gender,
            user.SexualPreference,
            user.Biography,
            InterestTags = string.Join(',', user.InterestTags),
            user.ProfilePhotoUrl,
            PhotoUrls = string.Join(',', user.PhotoUrls),
            user.Latitude,
            user.Longitude,
            user.FameRating,
            user.IsOnline,
            user.LastSeen,
            user.IsEmailVerified,
            user.EmailVerifiedAt,
            user.IsActive,
            user.DeactivatedAt
        };

        await connection.ExecuteAsync(sql, parameters);

        return user;
    }

    public async Task DeleteUserAsync(int id)
    {
        const string sql = "DELETE FROM users WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<List<User>> GetUserSuggestionsAsync(int userId, int limit = 10)
    {
        const string sql = @"
            WITH blocked_users AS (
                SELECT CASE
                    WHEN blocker_id = @UserId THEN blocked_id
                    ELSE blocker_id
                END AS user_id
                FROM blocks
                WHERE blocker_id = @UserId OR blocked_id = @UserId
            ),
            liked_users AS (
                SELECT liked_id AS user_id
                FROM likes
                WHERE liker_id = @UserId
            ),
            current_user_info AS (
                SELECT sexual_preference, latitude, longitude
                FROM users
                WHERE id = @UserId
            )
            SELECT
                u.id, u.username, u.email, u.first_name AS FirstName, u.last_name AS LastName,
                u.birth_date AS BirthDate, u.gender, u.sexual_preference AS SexualPreference,
                u.biography, u.profile_photo_url AS ProfilePhotoUrl,
                u.latitude, u.longitude, u.fame_rating AS FameRating,
                u.is_online AS IsOnline, u.last_seen AS LastSeen, u.is_email_verified AS IsEmailVerified,
                u.email_verified_at AS EmailVerifiedAt, u.is_active AS IsActive,
                u.deactivated_at AS DeactivatedAt, u.created_at AS CreatedAt,
                -- Haversine formula for distance calculation
                (6371 * acos(
                    cos(radians(cui.latitude)) * cos(radians(u.latitude)) *
                    cos(radians(u.longitude) - radians(cui.longitude)) +
                    sin(radians(cui.latitude)) * sin(radians(u.latitude))
                )) AS Distance
            FROM users u
            CROSS JOIN current_user_info cui
            WHERE u.id != @UserId
                AND u.id NOT IN (SELECT user_id FROM blocked_users)
                AND u.id NOT IN (SELECT user_id FROM liked_users)
                AND (
                    cui.sexual_preference = 'both'
                    OR cui.sexual_preference = ''
                    OR LOWER(u.gender) = LOWER(cui.sexual_preference)
                )
            ORDER BY Distance ASC, u.fame_rating DESC
            LIMIT @Limit
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var users = await connection.QueryAsync<User>(sql, new { UserId = userId, Limit = limit });

        return users.ToList();
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
        var sql = @"
            WITH blocked_users AS (
                SELECT CASE
                    WHEN blocker_id = @UserId THEN blocked_id
                    ELSE blocker_id
                END AS user_id
                FROM blocks
                WHERE blocker_id = @UserId OR blocked_id = @UserId
            ),
            current_user_info AS (
                SELECT latitude, longitude
                FROM users
                WHERE id = @UserId
            )
            SELECT
                u.id, u.username, u.email, u.first_name AS FirstName, u.last_name AS LastName,
                u.birth_date AS BirthDate, u.gender, u.sexual_preference AS SexualPreference,
                u.biography, u.profile_photo_url AS ProfilePhotoUrl,
                u.latitude, u.longitude, u.fame_rating AS FameRating,
                u.is_online AS IsOnline, u.last_seen AS LastSeen, u.is_email_verified AS IsEmailVerified,
                u.email_verified_at AS EmailVerifiedAt, u.is_active AS IsActive,
                u.deactivated_at AS DeactivatedAt, u.created_at AS CreatedAt,
                (6371 * acos(
                    cos(radians(cui.latitude)) * cos(radians(u.latitude)) *
                    cos(radians(u.longitude) - radians(cui.longitude)) +
                    sin(radians(cui.latitude)) * sin(radians(u.latitude))
                )) AS Distance
            FROM users u
            CROSS JOIN current_user_info cui
            WHERE u.id != @UserId
                AND u.id NOT IN (SELECT user_id FROM blocked_users)
                AND (@FameMin IS NULL OR u.fame_rating >= @FameMin)
                AND (@FameMax IS NULL OR u.fame_rating <= @FameMax)
                AND (@AgeMin IS NULL OR EXTRACT(YEAR FROM AGE(u.birth_date)) >= @AgeMin)
                AND (@AgeMax IS NULL OR EXTRACT(YEAR FROM AGE(u.birth_date)) <= @AgeMax)
            ORDER BY Distance ASC
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new
        {
            UserId = userId,
            FameMin = fameMin,
            FameMax = fameMax,
            AgeMin = ageMin,
            AgeMax = ageMax
        };

        var users = (await connection.QueryAsync<User>(sql, parameters)).ToList();

        // Filter by distance in memory
        if (distanceMax.HasValue)
        {
            users = users.Where(u => u.Distance <= distanceMax.Value).ToList();
        }

        // TODO: Re-implement tag filtering when InterestTags support is added back

        return users;
    }

    public async Task UpdateLastSeenAsync(int userId)
    {
        const string sql = @"
            UPDATE users
            SET last_seen = @LastSeen, is_online = true
            WHERE id = @UserId
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { UserId = userId, LastSeen = DateTime.UtcNow });
    }

    public async Task SetOfflineAsync(int userId)
    {
        const string sql = @"
            UPDATE users
            SET is_online = false, last_seen = @LastSeen
            WHERE id = @UserId
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { UserId = userId, LastSeen = DateTime.UtcNow });
    }

    public async Task UpdateFameRatingAsync(int userId)
    {
        const string sql = @"
            UPDATE users
            SET fame_rating = LEAST(100, (
                (SELECT COUNT(*) FROM likes WHERE liked_id = @UserId) * 10 +
                (SELECT COUNT(*) FROM profile_views WHERE viewed_id = @UserId) * 2 +
                (SELECT COUNT(*) FROM matches WHERE user1id = @UserId OR user2id = @UserId) * 20
            ))
            WHERE id = @UserId
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task<int> GetUsersCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM users";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetMatchesCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM matches";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetLikesCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM likes";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetNotificationsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM notifications";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task DeactivateUserAsync(int userId)
    {
        const string sql = @"
            UPDATE users
            SET is_active = false, deactivated_at = @DeactivatedAt
            WHERE id = @UserId
        ";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { UserId = userId, DeactivatedAt = DateTime.UtcNow });
    }

    public async Task DeleteUserCompletelyAsync(int userId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Delete all related data
            await connection.ExecuteAsync("DELETE FROM likes WHERE liker_id = @UserId OR liked_id = @UserId", new { UserId = userId }, transaction);
            await connection.ExecuteAsync("DELETE FROM matches WHERE user1id = @UserId OR user2id = @UserId", new { UserId = userId }, transaction);
            await connection.ExecuteAsync("DELETE FROM messages WHERE sender_id = @UserId OR receiver_id = @UserId", new { UserId = userId }, transaction);
            await connection.ExecuteAsync("DELETE FROM notifications WHERE user_id = @UserId", new { UserId = userId }, transaction);
            await connection.ExecuteAsync("DELETE FROM profile_views WHERE viewer_id = @UserId OR viewed_id = @UserId", new { UserId = userId }, transaction);
            await connection.ExecuteAsync("DELETE FROM reports WHERE reporter_id = @UserId OR reported_id = @UserId", new { UserId = userId }, transaction);
            await connection.ExecuteAsync("DELETE FROM blocks WHERE blocker_id = @UserId OR blocked_id = @UserId", new { UserId = userId }, transaction);
            await connection.ExecuteAsync("DELETE FROM user_passwords WHERE user_id = @UserId", new { UserId = userId }, transaction);
            await connection.ExecuteAsync("DELETE FROM users WHERE id = @UserId", new { UserId = userId }, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private List<string> ParseCsvField(string csvString)
    {
        if (string.IsNullOrWhiteSpace(csvString))
            return new List<string>();

        return csvString.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
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
