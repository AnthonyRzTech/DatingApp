using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;
using WebMatcha.Services;
using FluentAssertions;

namespace WebMatcha.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly MatchaDbContext _context;
    private readonly AuthService _authService;
    
    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<MatchaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new MatchaDbContext(options);
        _authService = new AuthService(_context);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUserAndPassword()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Today.AddYears(-25),
            Gender = "male",
            SexualPreference = "female"
        };
        
        // Act
        var result = await _authService.RegisterAsync(request);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Message.Should().Contain("success");
        
        // Verify user was created
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        user.Should().NotBeNull();
        user!.Email.Should().Be(request.Email);
        user.FirstName.Should().Be(request.FirstName);
        user.LastName.Should().Be(request.LastName);
        
        // Verify password was created and hashed
        var password = await _context.UserPasswords.FirstOrDefaultAsync(p => p.UserId == user.Id);
        password.Should().NotBeNull();
        password!.PasswordHash.Should().NotBe(request.Password);
        BCrypt.Net.BCrypt.Verify(request.Password, password.PasswordHash).Should().BeTrue();
    }
    
    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsError()
    {
        // Arrange
        var existingUser = new User
        {
            Username = "existinguser",
            Email = "existing@example.com",
            FirstName = "Existing",
            LastName = "User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = "male",
            SexualPreference = "female"
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();
        
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "new@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "TestPassword123!",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Today.AddYears(-25),
            Gender = "male",
            SexualPreference = "female"
        };
        
        // Act
        var result = await _authService.RegisterAsync(request);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Username or email already exists");
    }
    
    [Fact]
    public async Task RegisterAsync_WithPasswordMismatch_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPassword123!",
            ConfirmPassword = "DifferentPassword123!",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.Today.AddYears(-25),
            Gender = "male",
            SexualPreference = "female"
        };
        
        // Act
        var result = await _authService.RegisterAsync(request);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Passwords do not match");
    }
    
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessAndUser()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("TestPassword123!", 12);
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = "male",
            SexualPreference = "female"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var userPassword = new UserPassword
        {
            UserId = user.Id,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };
        _context.UserPasswords.Add(userPassword);
        await _context.SaveChangesAsync();
        
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "TestPassword123!"
        };
        
        // Act
        var result = await _authService.LoginAsync(request);
        
        // Assert
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("testuser");
        result.Message.Should().Contain("success");
        
        // Verify user was marked as online
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.IsOnline.Should().BeTrue();
    }
    
    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistentuser",
            Password = "TestPassword123!"
        };
        
        // Act
        var result = await _authService.LoginAsync(request);
        
        // Assert
        result.Success.Should().BeFalse();
        result.User.Should().BeNull();
        result.Errors.Should().Contain("Invalid credentials");
    }
    
    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsError()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!", 12);
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = "male",
            SexualPreference = "female"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var userPassword = new UserPassword
        {
            UserId = user.Id,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };
        _context.UserPasswords.Add(userPassword);
        await _context.SaveChangesAsync();
        
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword123!"
        };
        
        // Act
        var result = await _authService.LoginAsync(request);
        
        // Assert
        result.Success.Should().BeFalse();
        result.User.Should().BeNull();
        result.Errors.Should().Contain("Invalid credentials");
    }
    
    [Fact]
    public async Task LogoutAsync_UpdatesUserOfflineStatus()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = "male",
            SexualPreference = "female",
            IsOnline = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Act
        await _authService.LogoutAsync(user.Id);
        
        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.IsOnline.Should().BeFalse();
        updatedUser.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}