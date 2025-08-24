using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;
using WebMatcha.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace WebMatcha.Tests.Integration;

public class AuthenticationFlowTests : IDisposable
{
    private readonly MatchaDbContext _context;
    private readonly AuthService _authService;
    private readonly ServerSessionService _sessionService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<UserService> _mockUserService;
    private readonly DefaultHttpContext _httpContext;
    
    public AuthenticationFlowTests()
    {
        var options = new DbContextOptionsBuilder<MatchaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new MatchaDbContext(options);
        _authService = new AuthService(_context);
        
        // Setup HTTP context with session
        _httpContext = new DefaultHttpContext();
        _httpContext.Session = new TestSession();
        
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);
        
        _mockUserService = new Mock<UserService>(_context);
        _sessionService = new ServerSessionService(_mockHttpContextAccessor.Object, _mockUserService.Object);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    [Fact]
    public async Task CompleteAuthenticationFlow_RegisterThenLogin_WorksCorrectly()
    {
        // Step 1: Register a new user
        var registerRequest = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "New",
            LastName = "User",
            BirthDate = DateTime.Today.AddYears(-25),
            Gender = "male",
            SexualPreference = "female"
        };
        
        var registerResult = await _authService.RegisterAsync(registerRequest);
        registerResult.Success.Should().BeTrue();
        
        // Verify user exists in database
        var createdUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == "newuser");
        createdUser.Should().NotBeNull();
        
        // Step 2: Login with the registered user
        var loginRequest = new LoginRequest
        {
            Username = "newuser",
            Password = "SecurePassword123!"
        };
        
        var loginResult = await _authService.LoginAsync(loginRequest);
        loginResult.Success.Should().BeTrue();
        loginResult.User.Should().NotBeNull();
        loginResult.User!.Username.Should().Be("newuser");
        
        // Step 3: Set session
        _sessionService.SetCurrentUser(loginResult.User);
        
        // Step 4: Verify session
        _sessionService.IsAuthenticated().Should().BeTrue();
        _sessionService.GetCurrentUsername().Should().Be("newuser");
        _sessionService.GetCurrentUserId().Should().Be(createdUser!.Id);
        
        // Step 5: Logout
        await _authService.LogoutAsync(createdUser.Id);
        _sessionService.ClearSession();
        
        // Step 6: Verify logout
        _sessionService.IsAuthenticated().Should().BeFalse();
        _sessionService.GetCurrentUserId().Should().BeNull();
        
        var loggedOutUser = await _context.Users.FindAsync(createdUser.Id);
        loggedOutUser!.IsOnline.Should().BeFalse();
    }
    
    [Fact]
    public async Task AuthenticationFlow_CannotLoginWithoutRegistration()
    {
        // Try to login without registration
        var loginRequest = new LoginRequest
        {
            Username = "nonexistent",
            Password = "Password123!"
        };
        
        var loginResult = await _authService.LoginAsync(loginRequest);
        
        loginResult.Success.Should().BeFalse();
        loginResult.User.Should().BeNull();
        loginResult.Errors.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task AuthenticationFlow_CannotRegisterSameUserTwice()
    {
        // First registration
        var registerRequest = new RegisterRequest
        {
            Username = "duplicate",
            Email = "duplicate@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "Duplicate",
            LastName = "User",
            BirthDate = DateTime.Today.AddYears(-25),
            Gender = "female",
            SexualPreference = "male"
        };
        
        var firstResult = await _authService.RegisterAsync(registerRequest);
        firstResult.Success.Should().BeTrue();
        
        // Second registration with same username
        var secondResult = await _authService.RegisterAsync(registerRequest);
        secondResult.Success.Should().BeFalse();
        secondResult.Errors.Should().Contain("Username or email already exists");
    }
}

// Test session implementation
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();
    
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