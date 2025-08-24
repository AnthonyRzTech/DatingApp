using Xunit;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebMatcha.Components.Pages;
using WebMatcha.Services;
using WebMatcha.Models;
using WebMatcha.Tests.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace WebMatcha.Tests.Components;

public class RegisterPageTests : TestContext
{
    private readonly Mock<AuthService> _mockAuthService;
    private readonly Mock<NavigationManager> _mockNavigationManager;

    public RegisterPageTests()
    {
        _mockAuthService = new Mock<AuthService>(TestDbContextFactory.CreateInMemoryContext());
        _mockNavigationManager = new Mock<NavigationManager>();
        
        Services.AddSingleton(_mockAuthService.Object);
        Services.AddSingleton(_mockNavigationManager.Object);
    }

    [Fact]
    public void RegisterPage_RendersCorrectly()
    {
        // Act
        var component = RenderComponent<Register>();

        // Assert
        component.Find("h2").TextContent.Should().Contain("Join WebMatcha");
        component.Find("form").Should().NotBeNull();
        component.FindAll("input").Count.Should().BeGreaterThan(0);
        component.Find("button[type='submit']").TextContent.Should().Contain("Create Account");
    }

    [Fact]
    public void RegisterPage_DisplaysAllRequiredFields()
    {
        // Act
        var component = RenderComponent<Register>();

        // Assert
        component.Find("input[placeholder='Your first name']").Should().NotBeNull();
        component.Find("input[placeholder='Your last name']").Should().NotBeNull();
        component.Find("input[placeholder='Choose a username']").Should().NotBeNull();
        component.Find("input[placeholder='your@email.com']").Should().NotBeNull();
        component.Find("input[placeholder='Create a strong password']").Should().NotBeNull();
        component.Find("input[placeholder='Confirm your password']").Should().NotBeNull();
        component.FindAll("select").Count.Should().Be(2); // Gender and Sexual Preference
    }

    [Fact]
    public async Task RegisterPage_WithValidData_CallsAuthServiceAndRedirects()
    {
        // Arrange
        var successResult = new AuthResult
        {
            Success = true,
            Message = "Registration successful",
            User = TestHelpers.CreateTestUser()
        };

        _mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(successResult);

        var component = RenderComponent<Register>();

        // Act
        await component.Find("input[placeholder='Your first name']").ChangeAsync(new ChangeEventArgs { Value = "John" });
        await component.Find("input[placeholder='Your last name']").ChangeAsync(new ChangeEventArgs { Value = "Doe" });
        await component.Find("input[placeholder='Choose a username']").ChangeAsync(new ChangeEventArgs { Value = "johndoe" });
        await component.Find("input[placeholder='your@email.com']").ChangeAsync(new ChangeEventArgs { Value = "john@example.com" });
        await component.Find("input[placeholder='Create a strong password']").ChangeAsync(new ChangeEventArgs { Value = "Password123!" });
        await component.Find("input[placeholder='Confirm your password']").ChangeAsync(new ChangeEventArgs { Value = "Password123!" });
        
        var genderSelect = component.FindAll("select")[0];
        await genderSelect.ChangeAsync(new ChangeEventArgs { Value = "Male" });
        
        var preferenceSelect = component.FindAll("select")[1];
        await preferenceSelect.ChangeAsync(new ChangeEventArgs { Value = "Female" });

        var form = component.Find("form");
        await form.SubmitAsync();

        // Assert
        _mockAuthService.Verify(x => x.RegisterAsync(It.Is<RegisterRequest>(r =>
            r.FirstName == "John" &&
            r.LastName == "Doe" &&
            r.Username == "johndoe" &&
            r.Email == "john@example.com" &&
            r.Password == "Password123!" &&
            r.ConfirmPassword == "Password123!" &&
            r.Gender == "Male" &&
            r.SexualPreference == "Female"
        )), Times.Once);

        // Check success message is displayed
        component.WaitForAssertion(() =>
        {
            var successAlert = component.Find(".alert-success");
            successAlert.TextContent.Should().Contain("Account created successfully");
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RegisterPage_WithExistingUsername_DisplaysError()
    {
        // Arrange
        var errorResult = new AuthResult
        {
            Success = false,
            Message = "Username already exists",
            Errors = new List<string> { "Username already exists" }
        };

        _mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(errorResult);

        var component = RenderComponent<Register>();

        // Act - Fill minimum required fields
        await component.Find("input[placeholder='Your first name']").ChangeAsync(new ChangeEventArgs { Value = "John" });
        await component.Find("input[placeholder='Your last name']").ChangeAsync(new ChangeEventArgs { Value = "Doe" });
        await component.Find("input[placeholder='Choose a username']").ChangeAsync(new ChangeEventArgs { Value = "existinguser" });
        await component.Find("input[placeholder='your@email.com']").ChangeAsync(new ChangeEventArgs { Value = "john@example.com" });
        await component.Find("input[placeholder='Create a strong password']").ChangeAsync(new ChangeEventArgs { Value = "Password123!" });
        await component.Find("input[placeholder='Confirm your password']").ChangeAsync(new ChangeEventArgs { Value = "Password123!" });
        
        var genderSelect = component.FindAll("select")[0];
        await genderSelect.ChangeAsync(new ChangeEventArgs { Value = "Male" });
        
        var preferenceSelect = component.FindAll("select")[1];
        await preferenceSelect.ChangeAsync(new ChangeEventArgs { Value = "Female" });

        var form = component.Find("form");
        await form.SubmitAsync();

        // Assert
        component.WaitForAssertion(() =>
        {
            var errorAlert = component.Find(".alert-danger");
            errorAlert.TextContent.Should().Contain("Username already exists");
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RegisterPage_WithMultipleErrors_DisplaysAllErrors()
    {
        // Arrange
        var errorResult = new AuthResult
        {
            Success = false,
            Message = "Validation failed",
            Errors = new List<string> 
            { 
                "Password must be at least 6 characters",
                "Email is invalid",
                "Username is too short"
            }
        };

        _mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(errorResult);

        var component = RenderComponent<Register>();

        // Act - Submit form with some data
        await component.Find("input[placeholder='Your first name']").ChangeAsync(new ChangeEventArgs { Value = "John" });
        await component.Find("input[placeholder='Your last name']").ChangeAsync(new ChangeEventArgs { Value = "Doe" });
        await component.Find("input[placeholder='Choose a username']").ChangeAsync(new ChangeEventArgs { Value = "ab" });
        await component.Find("input[placeholder='your@email.com']").ChangeAsync(new ChangeEventArgs { Value = "invalid" });
        await component.Find("input[placeholder='Create a strong password']").ChangeAsync(new ChangeEventArgs { Value = "123" });
        await component.Find("input[placeholder='Confirm your password']").ChangeAsync(new ChangeEventArgs { Value = "123" });
        
        var genderSelect = component.FindAll("select")[0];
        await genderSelect.ChangeAsync(new ChangeEventArgs { Value = "Male" });
        
        var preferenceSelect = component.FindAll("select")[1];
        await preferenceSelect.ChangeAsync(new ChangeEventArgs { Value = "Female" });

        var form = component.Find("form");
        await form.SubmitAsync();

        // Assert
        component.WaitForAssertion(() =>
        {
            var errorAlert = component.Find(".alert-danger");
            errorAlert.TextContent.Should().Contain("Password must be at least 6 characters");
            errorAlert.TextContent.Should().Contain("Email is invalid");
            errorAlert.TextContent.Should().Contain("Username is too short");
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegisterPage_SubmitButton_ShowsLoadingStateWhenSubmitting()
    {
        // Arrange
        var tcs = new TaskCompletionSource<AuthResult>();
        _mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .Returns(tcs.Task);

        var component = RenderComponent<Register>();

        // Act - Fill and submit form
        component.Find("input[placeholder='Your first name']").Change("John");
        component.Find("input[placeholder='Your last name']").Change("Doe");
        component.Find("input[placeholder='Choose a username']").Change("johndoe");
        component.Find("input[placeholder='your@email.com']").Change("john@example.com");
        component.Find("input[placeholder='Create a strong password']").Change("Password123!");
        component.Find("input[placeholder='Confirm your password']").Change("Password123!");
        component.FindAll("select")[0].Change("Male");
        component.FindAll("select")[1].Change("Female");

        var form = component.Find("form");
        form.Submit();

        // Assert - Button should show loading state
        component.WaitForAssertion(() =>
        {
            var submitButton = component.Find("button[type='submit']");
            submitButton.TextContent.Should().Contain("Creating Account...");
            submitButton.GetAttribute("disabled").Should().NotBeNull();
        }, TimeSpan.FromSeconds(1));

        // Complete the task
        tcs.SetResult(new AuthResult { Success = true });
    }

    [Fact]
    public async Task RegisterPage_HandlesExceptionGracefully()
    {
        // Arrange
        _mockAuthService
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var component = RenderComponent<Register>();

        // Act - Fill and submit form
        await component.Find("input[placeholder='Your first name']").ChangeAsync(new ChangeEventArgs { Value = "John" });
        await component.Find("input[placeholder='Your last name']").ChangeAsync(new ChangeEventArgs { Value = "Doe" });
        await component.Find("input[placeholder='Choose a username']").ChangeAsync(new ChangeEventArgs { Value = "johndoe" });
        await component.Find("input[placeholder='your@email.com']").ChangeAsync(new ChangeEventArgs { Value = "john@example.com" });
        await component.Find("input[placeholder='Create a strong password']").ChangeAsync(new ChangeEventArgs { Value = "Password123!" });
        await component.Find("input[placeholder='Confirm your password']").ChangeAsync(new ChangeEventArgs { Value = "Password123!" });
        
        var genderSelect = component.FindAll("select")[0];
        await genderSelect.ChangeAsync(new ChangeEventArgs { Value = "Male" });
        
        var preferenceSelect = component.FindAll("select")[1];
        await preferenceSelect.ChangeAsync(new ChangeEventArgs { Value = "Female" });

        var form = component.Find("form");
        await form.SubmitAsync();

        // Assert
        component.WaitForAssertion(() =>
        {
            var errorAlert = component.Find(".alert-danger");
            errorAlert.TextContent.Should().Contain("An error occurred during registration");
        }, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegisterPage_HasLinkToLoginPage()
    {
        // Act
        var component = RenderComponent<Register>();

        // Assert
        var loginLink = component.Find("a[href='/login']");
        loginLink.Should().NotBeNull();
        loginLink.TextContent.Should().Contain("Sign In");
    }

    [Fact]
    public void RegisterPage_DefaultBirthDateIs25YearsAgo()
    {
        // Act
        var component = RenderComponent<Register>();

        // Assert
        var birthDateInput = component.Find("input[type='date']");
        var defaultValue = birthDateInput.GetAttribute("value");
        
        // The default date should be approximately 25 years ago
        if (!string.IsNullOrEmpty(defaultValue))
        {
            var date = DateTime.Parse(defaultValue);
            var expectedYear = DateTime.Today.Year - 25;
            date.Year.Should().Be(expectedYear);
        }
    }
}