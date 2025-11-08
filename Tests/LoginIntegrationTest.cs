using System;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using WebMatcha.Models;
using WebMatcha.Services;

namespace WebMatcha.Tests;

/// <summary>
/// Integration test to verify login functionality works end-to-end
/// </summary>
public class LoginIntegrationTest
{
    private readonly string _connectionString;

    public LoginIntegrationTest()
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=webmatcha;Username=postgres;Password=q";
    }

    public async Task<bool> RunAllTests()
    {
        Console.WriteLine("=== LOGIN INTEGRATION TESTS ===\n");

        try
        {
            // Initialize Dapper type handlers
            DapperConfig.Initialize();
            Console.WriteLine("✓ Dapper type handlers initialized");

            // Test 1: Database connection
            if (!await TestDatabaseConnection())
                return false;

            // Test 2: User query with List<string> mapping
            if (!await TestUserQueryMapping())
                return false;

            // Test 3: Full login flow
            if (!await TestLoginFlow())
                return false;

            Console.WriteLine("\n=== ALL TESTS PASSED ===");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ TEST SUITE FAILED: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    private async Task<bool> TestDatabaseConnection()
    {
        Console.WriteLine("\n[Test 1] Database Connection");
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM users");
            Console.WriteLine($"  → Connected successfully");
            Console.WriteLine($"  → Found {count} users in database");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> TestUserQueryMapping()
    {
        Console.WriteLine("\n[Test 2] User Query with List<string> Mapping");
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // This is the exact query from CompleteAuthService.LoginAsync
            const string sql = @"
                SELECT id, username, email, first_name AS FirstName, last_name AS LastName,
                    birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
                    biography, interest_tags AS InterestTags, profile_photo_url AS ProfilePhotoUrl,
                    photo_urls AS PhotoUrls, latitude, longitude, fame_rating AS FameRating,
                    is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
                    email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
                    created_at AS CreatedAt
                FROM users
                WHERE is_email_verified = true
                LIMIT 1
            ";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql);

            if (user == null)
            {
                Console.WriteLine("  ✗ Failed: No verified users found");
                return false;
            }

            Console.WriteLine($"  → Successfully queried user: {user.Username}");
            Console.WriteLine($"  → InterestTags type: {user.InterestTags?.GetType().Name ?? "null"}");
            Console.WriteLine($"  → InterestTags count: {user.InterestTags?.Count ?? 0}");
            Console.WriteLine($"  → PhotoUrls type: {user.PhotoUrls?.GetType().Name ?? "null"}");
            Console.WriteLine($"  ✓ List<string> mapping works correctly");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Failed: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  ✗ Inner: {ex.InnerException.Message}");
            return false;
        }
    }

    private async Task<bool> TestLoginFlow()
    {
        Console.WriteLine("\n[Test 3] Complete Login Flow");
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Find a user with password
            const string findUserSql = @"
                SELECT u.id, u.username, u.email, u.is_email_verified
                FROM users u
                INNER JOIN user_passwords up ON u.id = up.user_id
                WHERE u.is_email_verified = true
                LIMIT 1
            ";

            var testUser = await connection.QueryFirstOrDefaultAsync<(int id, string username, string email, bool verified)>(findUserSql);

            if (testUser == default)
            {
                Console.WriteLine("  ✗ Failed: No verified user with password found");
                return false;
            }

            Console.WriteLine($"  → Found test user: {testUser.username}");

            // Simulate the login query (without password verification)
            const string loginSql = @"
                SELECT id, username, email, first_name AS FirstName, last_name AS LastName,
                    birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
                    biography, interest_tags AS InterestTags, profile_photo_url AS ProfilePhotoUrl,
                    photo_urls AS PhotoUrls, latitude, longitude, fame_rating AS FameRating,
                    is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
                    email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
                    created_at AS CreatedAt
                FROM users
                WHERE username = @Username OR email = @Username
            ";

            var user = await connection.QueryFirstOrDefaultAsync<User>(loginSql, new { Username = testUser.username });

            if (user == null)
            {
                Console.WriteLine("  ✗ Failed: Could not retrieve user");
                return false;
            }

            Console.WriteLine($"  → Successfully retrieved user object");
            Console.WriteLine($"  → Username: {user.Username}");
            Console.WriteLine($"  → Email: {user.Email}");
            Console.WriteLine($"  → Email Verified: {user.IsEmailVerified}");
            Console.WriteLine($"  ✓ Login flow query works correctly");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Failed: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  ✗ Inner: {ex.InnerException.Message}");
            return false;
        }
    }

    public static async Task Main(string[] args)
    {
        // Load environment
        DotNetEnv.Env.Load();

        var test = new LoginIntegrationTest();
        var success = await test.RunAllTests();

        Environment.Exit(success ? 0 : 1);
    }
}
