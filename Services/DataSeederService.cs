using Npgsql;
using Dapper;
using WebMatcha.Models;

namespace WebMatcha.Services;

/// <summary>
/// DataSeederService - Refactoré avec requêtes SQL manuelles
/// CRITIQUE: Génère 500+ profils requis par le sujet
/// </summary>
public class DataSeederService
{
    private readonly string _connectionString;
    private readonly Random _random = new();

    private readonly string[] _maleNames = { "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Christopher", "Daniel", "Matthew", "Andrew", "Joshua", "Ryan", "Brandon", "Jason", "Justin", "Kevin", "Brian" };
    private readonly string[] _femaleNames = { "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen", "Nancy", "Lisa", "Betty", "Margaret", "Sandra", "Ashley", "Kimberly", "Emily", "Donna", "Michelle" };
    private readonly string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin" };
    private readonly string[] _interests = { "travel", "music", "movies", "sports", "reading", "cooking", "photography", "art", "fitness", "gaming", "dancing", "hiking", "yoga", "technology", "fashion", "wine", "coffee", "pets", "nature", "concerts" };
    private readonly string[] _biographies = {
        "Adventure seeker looking for someone to explore the world with.",
        "Coffee enthusiast and book lover. Let's discuss literature over lattes!",
        "Fitness fanatic who enjoys hiking and outdoor activities.",
        "Foodie who loves trying new restaurants and cooking at home.",
        "Music lover and concert-goer. What's your favorite band?",
        "Tech professional by day, gamer by night.",
        "Art enthusiast who enjoys museums and galleries.",
        "Travel addict with 20+ countries visited and counting!",
        "Dog parent looking for someone who loves animals.",
        "Netflix and chill? More like adventure and thrill!",
        "Passionate about photography and capturing moments.",
        "Yoga instructor seeking balance in life and love.",
        "Wine connoisseur who enjoys fine dining experiences.",
        "Outdoor enthusiast who loves camping and stargazing.",
        "Fashion designer with an eye for style and creativity."
    };

    public DataSeederService(IConfiguration configuration)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
    }

    public async Task SeedDatabaseAsync(int userCount = 500)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Check existing user count
        const string countSql = "SELECT COUNT(*) FROM users";
        var existingCount = await connection.ExecuteScalarAsync<int>(countSql);

        if (existingCount >= userCount)
        {
            Console.WriteLine($"Database already contains {existingCount} users. Skipping seed.");
            return;
        }

        var usersToAdd = userCount - existingCount;
        Console.WriteLine($"Database has {existingCount} users. Adding {usersToAdd} more users...");

        await GenerateUsersAsync(connection, usersToAdd);
        Console.WriteLine("Users created. Generating interactions...");

        await GenerateInteractionsAsync(connection);
        Console.WriteLine("Database seeding completed!");
    }

    private async Task GenerateUsersAsync(NpgsqlConnection connection, int count)
    {
        const string insertUserSql = @"
            INSERT INTO users (
                username, email, first_name, last_name, birth_date,
                gender, sexual_preference, biography, interest_tags,
                profile_photo_url, photo_urls, latitude, longitude,
                fame_rating, is_online, last_seen, is_email_verified,
                email_verified_at, is_active, created_at
            ) VALUES (
                @Username, @Email, @FirstName, @LastName, @BirthDate,
                @Gender, @SexualPreference, @Biography, @InterestTags,
                @ProfilePhotoUrl, @PhotoUrls, @Latitude, @Longitude,
                @FameRating, @IsOnline, @LastSeen, true,
                @CreatedAt, true, @CreatedAt
            )
        ";

        var parisLat = 48.8566;
        var parisLon = 2.3522;

        // Insert users in batches
        const int batchSize = 100;
        for (int batch = 0; batch < count; batch += batchSize)
        {
            var batchCount = Math.Min(batchSize, count - batch);
            var usersBatch = new List<object>();

            for (int i = 0; i < batchCount; i++)
            {
                var idx = batch + i + 1;
                var gender = _random.Next(2) == 0 ? "male" : "female";
                var firstName = gender == "male"
                    ? _maleNames[_random.Next(_maleNames.Length)]
                    : _femaleNames[_random.Next(_femaleNames.Length)];
                var lastName = _lastNames[_random.Next(_lastNames.Length)];

                var sexualPreferences = new[] { "male", "female", "both" };
                var sexualPreference = sexualPreferences[_random.Next(sexualPreferences.Length)];

                var birthYear = DateTime.UtcNow.Year - _random.Next(18, 51);
                var birthDate = new DateTime(birthYear, _random.Next(1, 13), _random.Next(1, 29), 0, 0, 0, DateTimeKind.Utc);

                var latitude = parisLat + (_random.NextDouble() - 0.5) * 2;
                var longitude = parisLon + (_random.NextDouble() - 0.5) * 2;

                var selectedInterests = new HashSet<string>();
                var interestCount = _random.Next(3, 8);
                while (selectedInterests.Count < interestCount)
                {
                    selectedInterests.Add(_interests[_random.Next(_interests.Length)]);
                }

                var photoUrls = new List<string>();
                var photoCount = _random.Next(1, 6);
                for (int p = 0; p < photoCount; p++)
                {
                    photoUrls.Add($"https://picsum.photos/seed/photo{idx}_{p}/400/600");
                }

                usersBatch.Add(new
                {
                    Username = $"{firstName.ToLower()}{lastName.ToLower()}{idx}",
                    Email = $"{firstName.ToLower()}.{lastName.ToLower()}{idx}@example.com",
                    FirstName = firstName,
                    LastName = lastName,
                    BirthDate = birthDate,
                    Gender = gender,
                    SexualPreference = sexualPreference,
                    Biography = _biographies[_random.Next(_biographies.Length)],
                    InterestTags = string.Join(',', selectedInterests),
                    ProfilePhotoUrl = $"https://picsum.photos/seed/user{idx}/300/300",
                    PhotoUrls = string.Join(',', photoUrls),
                    Latitude = latitude,
                    Longitude = longitude,
                    FameRating = _random.Next(0, 51),
                    IsOnline = _random.Next(10) < 3,
                    LastSeen = DateTime.UtcNow.AddHours(-_random.Next(0, 168)),
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 365))
                });
            }

            await connection.ExecuteAsync(insertUserSql, usersBatch);
            Console.WriteLine($"Inserted users {batch + 1} to {batch + batchCount}");
        }
    }

    private async Task GenerateInteractionsAsync(NpgsqlConnection connection)
    {
        // Get all user IDs
        const string getUserIdsSql = "SELECT id FROM users ORDER BY id";
        var userIds = (await connection.QueryAsync<int>(getUserIdsSql)).ToList();
        var userCount = userIds.Count;

        if (userCount < 2)
        {
            Console.WriteLine("Not enough users to generate interactions");
            return;
        }

        // Generate likes and matches
        var likes = new List<object>();
        var matches = new List<object>();
        var processedMatches = new HashSet<string>();

        foreach (var userId in userIds.Take(Math.Min(150, userCount)))
        {
            var likeCount = _random.Next(5, 20);
            var likedUsers = new HashSet<int>();

            for (int i = 0; i < likeCount; i++)
            {
                var targetUserId = userIds[_random.Next(userCount)];
                if (targetUserId != userId && !likedUsers.Contains(targetUserId))
                {
                    likedUsers.Add(targetUserId);

                    likes.Add(new
                    {
                        LikerId = userId,
                        LikedId = targetUserId,
                        CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30))
                    });

                    // 30% chance of reciprocal like (creating a match)
                    if (_random.Next(100) < 30)
                    {
                        likes.Add(new
                        {
                            LikerId = targetUserId,
                            LikedId = userId,
                            CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30))
                        });

                        var user1Id = Math.Min(userId, targetUserId);
                        var user2Id = Math.Max(userId, targetUserId);
                        var matchKey = $"{user1Id}-{user2Id}";

                        if (!processedMatches.Contains(matchKey))
                        {
                            processedMatches.Add(matchKey);
                            matches.Add(new
                            {
                                User1Id = user1Id,
                                User2Id = user2Id,
                                MatchedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 25))
                            });
                        }
                    }
                }
            }
        }

        // Insert likes
        if (likes.Any())
        {
            const string insertLikeSql = "INSERT INTO likes (liker_id, liked_id, created_at) VALUES (@LikerId, @LikedId, @CreatedAt)";
            await connection.ExecuteAsync(insertLikeSql, likes);
            Console.WriteLine($"Inserted {likes.Count} likes");
        }

        // Insert matches
        if (matches.Any())
        {
            const string insertMatchSql = "INSERT INTO matches (user1_id, user2_id, matched_at) VALUES (@User1Id, @User2Id, @MatchedAt) ON CONFLICT DO NOTHING";
            await connection.ExecuteAsync(insertMatchSql, matches);
            Console.WriteLine($"Inserted {matches.Count} matches");
        }

        // Generate profile views
        var views = new List<object>();
        foreach (var userId in userIds.Take(Math.Min(150, userCount)))
        {
            var viewCount = _random.Next(10, 30);
            var viewedUsers = new HashSet<int>();

            for (int i = 0; i < viewCount; i++)
            {
                var targetUserId = userIds[_random.Next(userCount)];
                if (targetUserId != userId && !viewedUsers.Contains(targetUserId))
                {
                    viewedUsers.Add(targetUserId);
                    views.Add(new
                    {
                        ViewerId = userId,
                        ViewedId = targetUserId,
                        ViewedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 7))
                    });
                }
            }
        }

        if (views.Any())
        {
            const string insertViewSql = "INSERT INTO profile_views (viewer_id, viewed_id, viewed_at) VALUES (@ViewerId, @ViewedId, @ViewedAt)";
            await connection.ExecuteAsync(insertViewSql, views);
            Console.WriteLine($"Inserted {views.Count} profile views");
        }

        // Generate notifications
        var notifications = new List<object>();
        var types = new[] { "like", "view", "match", "message" };

        foreach (var userId in userIds.Take(Math.Min(200, userCount)))
        {
            var notificationCount = _random.Next(3, 10);
            for (int i = 0; i < notificationCount; i++)
            {
                var type = types[_random.Next(types.Length)];
                var message = type switch
                {
                    "like" => "Someone liked your profile",
                    "view" => "Someone viewed your profile",
                    "match" => "You have a new match!",
                    "message" => "You have a new message",
                    _ => "New activity on your profile"
                };

                notifications.Add(new
                {
                    UserId = userId,
                    Type = type,
                    Message = message,
                    IsRead = _random.Next(100) < 70,
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 14))
                });
            }
        }

        if (notifications.Any())
        {
            const string insertNotificationSql = "INSERT INTO notifications (user_id, type, message, is_read, created_at) VALUES (@UserId, @Type, @Message, @IsRead, @CreatedAt)";
            await connection.ExecuteAsync(insertNotificationSql, notifications);
            Console.WriteLine($"Inserted {notifications.Count} notifications");
        }
    }
}
