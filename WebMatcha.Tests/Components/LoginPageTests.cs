using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebMatcha.Components.Pages;
using WebMatcha.Services;
using WebMatcha.Models;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace WebMatcha.Tests.Components;

public class LoginPageTests : TestContext
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IServerSessionService> _mockServerSession;
    private readonly Mock<NavigationManager> _mockNavigation;
    
    public LoginPageTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockServerSession = new Mock<IServerSessionService>();
        _mockNavigation = new Mock<NavigationManager>();
        
        Services.AddSingleton(_mockAuthService.Object);
        Services.AddSingleton(_mockServerSession.Object);
        Services.AddSingleton(_mockNavigation.Object);
    }
    
    [Fact]
    public void LoginPage_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<Login>();
        
        // Assert
        component.Find("h2").TextContent.Should().Contain("Welcome Back");
        component.FindAll("input").Should().HaveCount(2); // Username and Password
        component.Find("button[type='submit']").Should().NotBeNull();
    }
    
    [Fact]
    public void LoginPage_ShowsValidationErrors_WhenFieldsAreEmpty()
    {
        // Arrange
        var component = RenderComponent<Login>();
        
        // Act
        var form = component.Find("form");
        form.Submit();
        
        // Assert
        var validationMessages = component.FindAll(".validation-message");
        validationMessages.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task LoginPage_CallsAuthService_WithCorrectCredentials()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "TestPassword123!"
        };
        
        var authResult = new AuthResult
        {
            Success = true,
            User = new User { Id = 1, Username = "testuser" },
            Message = "Login successful"
        };
        
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(authResult);
        
        var component = RenderComponent<Login>();
        
        // Act
        component.Find("input[placeholder*='username']").Change("testuser");
        component.Find("input[type='password']").Change("TestPassword123!");
        await component.Find("form").SubmitAsync();
        
        // Assert
        _mockAuthService.Verify(x => x.LoginAsync(
            It.Is<LoginRequest>(r => r.Username == "testuser" && r.Password == "TestPassword123!")), 
            Times.Once);
    }
    
    [Fact]
    public async Task LoginPage_ShowsSuccessMessage_OnSuccessfulLogin()
    {
        // Arrange
        var authResult = new AuthResult
        {
            Success = true,
            User = new User { Id = 1, Username = "testuser" },
            Message = "Login successful"
        };
        
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(authResult);
        
        var component = RenderComponent<Login>();
        
        // Act
        component.Find("input[placeholder*='username']").Change("testuser");
        component.Find("input[type='password']").Change("TestPassword123!");
        await component.Find("form").SubmitAsync();
        
        // Assert
        component.Find(".alert-success").TextContent.Should().Contain("Login successful");
    }
    
    [Fact]
    public async Task LoginPage_ShowsErrorMessage_OnFailedLogin()
    {
        // Arrange
        var authResult = new AuthResult
        {
            Success = false,
            Errors = new List<string> { "Invalid username or password" }
        };
        
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(authResult);
        
        var component = RenderComponent<Login>();
        
        // Act
        component.Find("input[placeholder*='username']").Change("wronguser");
        component.Find("input[type='password']").Change("WrongPassword!");
        await component.Find("form").SubmitAsync();
        
        // Assert
        component.Find(".alert-danger").TextContent.Should().Contain("Invalid username or password");
    }
    
    [Fact]
    public async Task LoginPage_DisablesSubmitButton_WhileProcessing()
    {
        // Arrange
        var tcs = new TaskCompletionSource<AuthResult>();
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
            .Returns(tcs.Task);
        
        var component = RenderComponent<Login>();
        
        // Act
        component.Find("input[placeholder*='username']").Change("testuser");
        component.Find("input[type='password']").Change("TestPassword123!");
        var submitTask = component.Find("form").SubmitAsync();
        
        // Assert - button should be disabled while processing
        component.Find("button[type='submit']").GetAttribute("disabled").Should().NotBeNull();
        
        // Complete the task
        tcs.SetResult(new AuthResult { Success = true, User = new User() });
        await submitTask;
        
        // Button should be enabled again
        component.Find("button[type='submit']").GetAttribute("disabled").Should().BeNull();
    }
}