using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;

namespace WebMatcha.Tests.Utilities;

public static class TestDbContextFactory
{
    public static MatchaDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MatchaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MatchaDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static async Task<MatchaDbContext> CreateWithSeedDataAsync()
    {
        var context = CreateInMemoryContext();
        
        // Add test users
        var testUser1 = new WebMatcha.Models.User
        {
            Username = "testuser1",
            Email = "test1@example.com",
            FirstName = "Test",
            LastName = "User1",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = "Male",
            SexualPreference = "Female",
            Biography = "Test bio",
            InterestTags = new List<string> { "music", "movies" },
            ProfilePhotoUrl = "/images/default-avatar.png",
            PhotoUrls = new List<string>(),
            Latitude = 40.7128,
            Longitude = -74.0060,
            FameRating = 50,
            IsOnline = false,
            LastSeen = DateTime.UtcNow
        };

        var testUser2 = new WebMatcha.Models.User
        {
            Username = "testuser2",
            Email = "test2@example.com",
            FirstName = "Test",
            LastName = "User2",
            BirthDate = DateTime.UtcNow.AddYears(-23),
            Gender = "Female",
            SexualPreference = "Male",
            Biography = "Another test bio",
            InterestTags = new List<string> { "sports", "travel" },
            ProfilePhotoUrl = "/images/default-avatar.png",
            PhotoUrls = new List<string>(),
            Latitude = 40.7128,
            Longitude = -74.0060,
            FameRating = 75,
            IsOnline = false,
            LastSeen = DateTime.UtcNow
        };

        context.Users.AddRange(testUser1, testUser2);
        await context.SaveChangesAsync();

        // Add password for testuser1
        var passwordEntry = new WebMatcha.Services.UserPassword
        {
            UserId = testUser1.Id,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            CreatedAt = DateTime.UtcNow
        };

        context.UserPasswords.Add(passwordEntry);
        await context.SaveChangesAsync();

        return context;
    }
}