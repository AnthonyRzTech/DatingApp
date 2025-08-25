using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;

namespace WebMatcha.Services;

public class DataSeederService
{
    private readonly MatchaDbContext _context;
    private readonly Random _random = new();
    
    private readonly string[] _maleNames = { "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Christopher" };
    private readonly string[] _femaleNames = { "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen" };
    private readonly string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
    private readonly string[] _interests = { "travel", "music", "movies", "sports", "reading", "cooking", "photography", "art", "fitness", "gaming", "dancing", "hiking", "yoga", "technology", "fashion" };
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
        "Netflix and chill? More like adventure and thrill!"
    };
    
    public DataSeederService(MatchaDbContext context)
    {
        _context = context;
    }
    
    public async Task SeedDatabaseAsync(int userCount = 500)
    {
        var existingCount = await _context.Users.CountAsync();
        if (existingCount >= userCount)
        {
            Console.WriteLine($"Database already contains {existingCount} users. Skipping seed.");
            return;
        }
        
        var usersToAdd = userCount - existingCount;
        Console.WriteLine($"Database has {existingCount} users. Adding {usersToAdd} more users...");
        
        var users = GenerateUsers(usersToAdd);
        
        // Insert users in smaller batches to avoid parameter limit
        const int batchSize = 50;
        for (int i = 0; i < users.Count; i += batchSize)
        {
            var batch = users.Skip(i).Take(batchSize).ToList();
            await _context.Users.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Inserted users {i + 1} to {Math.Min(i + batchSize, users.Count)}");
        }
        
        Console.WriteLine("Users created. Generating interactions...");
        
        await GenerateInteractions();
        
        Console.WriteLine("Database seeding completed!");
    }
    
    private List<User> GenerateUsers(int count)
    {
        var users = new List<User>();
        var parisLat = 48.8566;
        var parisLon = 2.3522;
        
        for (int i = 1; i <= count; i++)
        {
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
            
            var selectedInterests = new List<string>();
            var interestCount = _random.Next(3, 8);
            for (int j = 0; j < interestCount; j++)
            {
                var interest = _interests[_random.Next(_interests.Length)];
                if (!selectedInterests.Contains(interest))
                {
                    selectedInterests.Add(interest);
                }
            }
            
            var user = new User
            {
                Username = $"{firstName.ToLower()}{lastName.ToLower()}{i}",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@example.com",
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate,
                Gender = gender,
                SexualPreference = sexualPreference,
                Biography = _biographies[_random.Next(_biographies.Length)],
                InterestTags = selectedInterests,
                ProfilePhotoUrl = $"https://picsum.photos/seed/user{i}/300/300",
                PhotoUrls = GeneratePhotoUrls(i),
                Latitude = latitude,
                Longitude = longitude,
                FameRating = _random.Next(0, 51),
                IsOnline = _random.Next(10) < 3,
                LastSeen = DateTime.UtcNow.AddHours(-_random.Next(0, 168))
            };
            
            users.Add(user);
        }
        
        return users;
    }
    
    private List<string> GeneratePhotoUrls(int userId)
    {
        var photoCount = _random.Next(1, 6);
        var urls = new List<string>();
        
        for (int i = 0; i < photoCount; i++)
        {
            urls.Add($"https://picsum.photos/seed/photo{userId}_{i}/400/600");
        }
        
        return urls;
    }
    
    private async Task GenerateInteractions()
    {
        var users = await _context.Users.ToListAsync();
        var userCount = users.Count;
        
        var batchCounter = 0;
        const int batchSize = 100;
        
        foreach (var user in users.Take(100))
        {
            var likeCount = _random.Next(5, 20);
            var likedUsers = new HashSet<int>();
            
            for (int i = 0; i < likeCount; i++)
            {
                var targetUser = users[_random.Next(userCount)];
                if (targetUser.Id != user.Id && !likedUsers.Contains(targetUser.Id))
                {
                    likedUsers.Add(targetUser.Id);
                    
                    var like = new Like
                    {
                        LikerId = user.Id,
                        LikedId = targetUser.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30))
                    };
                    
                    _context.Likes.Add(like);
                    
                    if (_random.Next(100) < 30)
                    {
                        // Check if reciprocal like doesn't already exist
                        var existingReciprocal = _context.Likes.Local
                            .Any(l => l.LikerId == targetUser.Id && l.LikedId == user.Id);
                        
                        if (!existingReciprocal)
                        {
                            var reciprocalLike = new Like
                            {
                                LikerId = targetUser.Id,
                                LikedId = user.Id,
                                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30))
                            };
                            
                            _context.Likes.Add(reciprocalLike);
                            
                            // Check if match doesn't already exist
                            var existingMatch = _context.Matches.Local
                                .Any(m => (m.User1Id == Math.Min(user.Id, targetUser.Id) && 
                                          m.User2Id == Math.Max(user.Id, targetUser.Id)));
                            
                            if (!existingMatch)
                            {
                                var match = new Match
                                {
                                    User1Id = Math.Min(user.Id, targetUser.Id),
                                    User2Id = Math.Max(user.Id, targetUser.Id),
                                    MatchedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 25))
                                };
                                
                                _context.Matches.Add(match);
                            }
                        }
                    }
                }
            }
            
            var viewCount = _random.Next(10, 30);
            var viewedUsers = new HashSet<int>();
            
            for (int i = 0; i < viewCount; i++)
            {
                var targetUser = users[_random.Next(userCount)];
                if (targetUser.Id != user.Id && !viewedUsers.Contains(targetUser.Id))
                {
                    viewedUsers.Add(targetUser.Id);
                    
                    var view = new ProfileView
                    {
                        ViewerId = user.Id,
                        ViewedId = targetUser.Id,
                        ViewedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 7))
                    };
                    
                    _context.ProfileViews.Add(view);
                }
            }
            
            batchCounter++;
            if (batchCounter % batchSize == 0)
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Saved batch of interactions for {batchCounter} users");
            }
        }
        
        // Save any remaining interactions
        await _context.SaveChangesAsync();
        
        // Add notifications in batches
        batchCounter = 0;
        foreach (var user in users.Take(200))
        {
            var notificationCount = _random.Next(3, 10);
            for (int i = 0; i < notificationCount; i++)
            {
                var types = new[] { "like", "view", "match" };
                var type = types[_random.Next(types.Length)];
                
                var message = type switch
                {
                    "like" => "Someone liked your profile",
                    "view" => "Someone viewed your profile",
                    "match" => "You have a new match!",
                    _ => "New activity on your profile"
                };
                
                var notification = new Notification
                {
                    UserId = user.Id,
                    Type = type,
                    Message = message,
                    IsRead = _random.Next(100) < 70,
                    CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 14))
                };
                
                _context.Notifications.Add(notification);
            }
            
            batchCounter++;
            if (batchCounter % batchSize == 0)
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Saved batch of notifications for {batchCounter} users");
            }
        }
        
        await _context.SaveChangesAsync();
    }
}