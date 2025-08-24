using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebMatcha.Models;
using WebMatcha.Services;

namespace WebMatcha.Tests.Utilities;

public static class TestHelpers
{
    public static RegisterRequest CreateValidRegisterRequest(string username = "newuser")
    {
        return new RegisterRequest
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            BirthDate = DateTime.Now.AddYears(-25),
            Gender = "Male",
            SexualPreference = "Female"
        };
    }

    public static LoginRequest CreateValidLoginRequest(string username = "testuser", string password = "Password123!")
    {
        return new LoginRequest
        {
            Username = username,
            Password = password
        };
    }

    public static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor()
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        
        // Configure session
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        services.AddSession();
        var serviceProvider = services.BuildServiceProvider();
        
        context.RequestServices = serviceProvider;
        context.Session = new TestSession();
        
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
        return mockHttpContextAccessor;
    }

    public static User CreateTestUser(int id = 1, string username = "testuser")
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = $"{username}@example.com",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = "Male",
            SexualPreference = "Female",
            Biography = "Test biography",
            InterestTags = new List<string> { "test", "user" },
            ProfilePhotoUrl = "/images/default-avatar.png",
            PhotoUrls = new List<string>(),
            Latitude = 0,
            Longitude = 0,
            FameRating = 50,
            IsOnline = false,
            LastSeen = DateTime.UtcNow
        };
    }
}

// Simple test session implementation
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _sessionStorage = new();

    public bool IsAvailable => true;
    public string Id => Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _sessionStorage.Keys;

    public void Clear() => _sessionStorage.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _sessionStorage.Remove(key);

    public void Set(string key, byte[] value) => _sessionStorage[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value);
}