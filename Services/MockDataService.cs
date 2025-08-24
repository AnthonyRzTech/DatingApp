using WebMatcha.Models;

namespace WebMatcha.Services;

public class MockDataService
{
    private readonly List<User> _users = new();
    private readonly List<Like> _likes = new();
    private readonly List<Match> _matches = new();
    private readonly List<ProfileView> _profileViews = new();
    private readonly List<Block> _blocks = new();
    private readonly List<Report> _reports = new();
    private readonly List<Notification> _notifications = new();
    private int _currentUserId = 1; // Simulated logged-in user
    
    public MockDataService()
    {
        InitializeMockData();
    }
    
    private void InitializeMockData()
    {
        var random = new Random();
        var firstNames = new[] { "Emma", "Liam", "Olivia", "Noah", "Ava", "Ethan", "Sophia", "Mason", "Isabella", "William",
            "Mia", "James", "Charlotte", "Benjamin", "Amelia", "Lucas", "Harper", "Henry", "Evelyn", "Alexander" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
        var bios = new[] { 
            "Adventure seeker and coffee enthusiast ☕", 
            "Love hiking and good conversations 🏔️",
            "Foodie | Travel addict | Dog lover 🐕",
            "Looking for someone to share sunsets with 🌅",
            "Gym rat by day, Netflix binger by night 💪",
            "Bookworm seeking my next chapter 📚",
            "Music is my therapy 🎵",
            "Life is too short for bad coffee ☕",
            "Wanderlust and city dust 🌍",
            "Here for a good time and a long time ✨"
        };
        var tags = new[] { "#travel", "#foodie", "#fitness", "#music", "#movies", "#reading", "#hiking", "#photography", "#art", "#cooking",
            "#gaming", "#yoga", "#dancing", "#nature", "#coffee", "#wine", "#dogs", "#cats", "#adventure", "#beach" };
        var genders = new[] { "Male", "Female", "Non-binary" };
        var preferences = new[] { "Male", "Female", "Both" };
        
        // Create 50 mock users
        for (int i = 1; i <= 50; i++)
        {
            var user = new User
            {
                Id = i,
                Username = $"user{i}",
                Email = $"user{i}@example.com",
                FirstName = firstNames[random.Next(firstNames.Length)],
                LastName = lastNames[random.Next(lastNames.Length)],
                BirthDate = DateTime.Now.AddYears(-random.Next(18, 45)).AddDays(-random.Next(365)),
                Gender = genders[random.Next(genders.Length)],
                SexualPreference = preferences[random.Next(preferences.Length)],
                Biography = bios[random.Next(bios.Length)],
                InterestTags = Enumerable.Range(0, random.Next(3, 7))
                    .Select(_ => tags[random.Next(tags.Length)])
                    .Distinct()
                    .ToList(),
                ProfilePhotoUrl = $"https://i.pravatar.cc/300?img={i}",
                PhotoUrls = new List<string> { 
                    $"https://i.pravatar.cc/300?img={i}",
                    $"https://picsum.photos/300/400?random={i}1",
                    $"https://picsum.photos/300/400?random={i}2"
                },
                Latitude = 48.8566 + (random.NextDouble() - 0.5) * 0.2, // Paris area
                Longitude = 2.3522 + (random.NextDouble() - 0.5) * 0.2,
                FameRating = random.Next(1, 6),
                IsOnline = random.Next(10) > 5,
                LastSeen = DateTime.Now.AddMinutes(-random.Next(1, 1440))
            };
            
            _users.Add(user);
        }
        
        // Add some initial likes
        for (int i = 0; i < 20; i++)
        {
            _likes.Add(new Like
            {
                Id = i + 1,
                LikerId = random.Next(2, 51),
                LikedId = 1,
                CreatedAt = DateTime.Now.AddDays(-random.Next(30))
            });
        }
        
        // Add some profile views
        for (int i = 0; i < 30; i++)
        {
            _profileViews.Add(new ProfileView
            {
                Id = i + 1,
                ViewerId = random.Next(2, 51),
                ViewedId = 1,
                ViewedAt = DateTime.Now.AddHours(-random.Next(72))
            });
        }
    }
    
    public int CurrentUserId => _currentUserId;
    
    public User? GetCurrentUser() => _users.FirstOrDefault(u => u.Id == _currentUserId);
    
    public List<User> GetAllUsers() => _users.Where(u => u.Id != _currentUserId).ToList();
    
    public User? GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);
    
    public List<User> GetSuggestedUsers()
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null) return new List<User>();
        
        var blockedUserIds = _blocks.Where(b => b.BlockerId == _currentUserId).Select(b => b.BlockedId).ToHashSet();
        
        return _users
            .Where(u => u.Id != _currentUserId && !blockedUserIds.Contains(u.Id))
            .Select(u => {
                u.Distance = CalculateDistance(currentUser.Latitude, currentUser.Longitude, u.Latitude, u.Longitude);
                return u;
            })
            .OrderBy(u => u.Distance)
            .Take(20)
            .ToList();
    }
    
    public bool IsLiked(int userId) => _likes.Any(l => l.LikerId == _currentUserId && l.LikedId == userId);
    
    public bool IsMatched(int userId)
    {
        return _matches.Any(m => 
            (m.User1Id == _currentUserId && m.User2Id == userId) ||
            (m.User1Id == userId && m.User2Id == _currentUserId));
    }
    
    public void LikeUser(int userId)
    {
        if (!IsLiked(userId))
        {
            var like = new Like
            {
                Id = _likes.Count + 1,
                LikerId = _currentUserId,
                LikedId = userId,
                CreatedAt = DateTime.Now
            };
            _likes.Add(like);
            
            // Check for mutual like (match)
            if (_likes.Any(l => l.LikerId == userId && l.LikedId == _currentUserId))
            {
                CreateMatch(_currentUserId, userId);
            }
            
            // Add notification
            AddNotification(userId, "like", $"{GetCurrentUser()?.FirstName} liked your profile!");
        }
    }
    
    public void UnlikeUser(int userId)
    {
        var like = _likes.FirstOrDefault(l => l.LikerId == _currentUserId && l.LikedId == userId);
        if (like != null)
        {
            _likes.Remove(like);
            
            // Remove match if exists
            var match = _matches.FirstOrDefault(m => 
                (m.User1Id == _currentUserId && m.User2Id == userId) ||
                (m.User1Id == userId && m.User2Id == _currentUserId));
            if (match != null)
            {
                _matches.Remove(match);
                AddNotification(userId, "unlike", $"{GetCurrentUser()?.FirstName} unmatched with you");
            }
        }
    }
    
    private void CreateMatch(int user1Id, int user2Id)
    {
        var match = new Match
        {
            Id = _matches.Count + 1,
            User1Id = Math.Min(user1Id, user2Id),
            User2Id = Math.Max(user1Id, user2Id),
            MatchedAt = DateTime.Now
        };
        _matches.Add(match);
        
        // Add notifications for both users
        var user1 = GetUserById(user1Id);
        var user2 = GetUserById(user2Id);
        AddNotification(user1Id, "match", $"You matched with {user2?.FirstName}! 🎉");
        AddNotification(user2Id, "match", $"You matched with {user1?.FirstName}! 🎉");
    }
    
    public void ViewProfile(int userId)
    {
        if (!_profileViews.Any(v => v.ViewerId == _currentUserId && v.ViewedId == userId && 
            v.ViewedAt > DateTime.Now.AddHours(-1)))
        {
            _profileViews.Add(new ProfileView
            {
                Id = _profileViews.Count + 1,
                ViewerId = _currentUserId,
                ViewedId = userId,
                ViewedAt = DateTime.Now
            });
            
            AddNotification(userId, "view", $"{GetCurrentUser()?.FirstName} viewed your profile");
        }
    }
    
    public void BlockUser(int userId)
    {
        if (!_blocks.Any(b => b.BlockerId == _currentUserId && b.BlockedId == userId))
        {
            _blocks.Add(new Block
            {
                Id = _blocks.Count + 1,
                BlockerId = _currentUserId,
                BlockedId = userId,
                CreatedAt = DateTime.Now
            });
            
            // Remove any existing match
            var match = _matches.FirstOrDefault(m => 
                (m.User1Id == _currentUserId && m.User2Id == userId) ||
                (m.User1Id == userId && m.User2Id == _currentUserId));
            if (match != null)
            {
                _matches.Remove(match);
            }
        }
    }
    
    public void ReportUser(int userId, string reason)
    {
        _reports.Add(new Report
        {
            Id = _reports.Count + 1,
            ReporterId = _currentUserId,
            ReportedId = userId,
            Reason = reason,
            CreatedAt = DateTime.Now
        });
    }
    
    public List<User> GetMatches()
    {
        var matchedUserIds = _matches
            .Where(m => m.User1Id == _currentUserId || m.User2Id == _currentUserId)
            .Select(m => m.User1Id == _currentUserId ? m.User2Id : m.User1Id)
            .ToHashSet();
        
        return _users.Where(u => matchedUserIds.Contains(u.Id)).ToList();
    }
    
    public List<User> GetLikedByUsers()
    {
        var likerIds = _likes
            .Where(l => l.LikedId == _currentUserId)
            .Select(l => l.LikerId)
            .ToHashSet();
        
        return _users.Where(u => likerIds.Contains(u.Id)).ToList();
    }
    
    public List<User> GetViewedByUsers()
    {
        var viewerIds = _profileViews
            .Where(v => v.ViewedId == _currentUserId)
            .OrderByDescending(v => v.ViewedAt)
            .Select(v => v.ViewerId)
            .Distinct()
            .ToHashSet();
        
        return _users.Where(u => viewerIds.Contains(u.Id)).ToList();
    }
    
    public List<Notification> GetNotifications()
    {
        return _notifications
            .Where(n => n.UserId == _currentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }
    
    public int GetUnreadNotificationCount()
    {
        return _notifications.Count(n => n.UserId == _currentUserId && !n.IsRead);
    }
    
    public void MarkNotificationsAsRead()
    {
        foreach (var notification in _notifications.Where(n => n.UserId == _currentUserId && !n.IsRead))
        {
            notification.IsRead = true;
        }
    }
    
    private void AddNotification(int userId, string type, string message)
    {
        _notifications.Add(new Notification
        {
            Id = _notifications.Count + 1,
            UserId = userId,
            Type = type,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.Now
        });
    }
    
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
    
    private double ToRadians(double degrees) => degrees * (Math.PI / 180);
}