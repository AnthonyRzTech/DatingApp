using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;
using BCrypt.Net;
using System.Security.Cryptography;

namespace WebMatcha.Services;

public class CompleteAuthService : IAuthService
{
    private readonly MatchaDbContext _context;
    private readonly IEmailService _emailService;
    
    public CompleteAuthService(MatchaDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }
    
    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var errors = ValidateRegistration(request);
        if (errors.Any())
        {
            return new AuthResult
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            };
        }
        
        // Check if username or email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
            
        if (existingUser != null)
        {
            var error = existingUser.Username == request.Username 
                ? "Username already exists" 
                : "Email already exists";
            return new AuthResult
            {
                Success = false,
                Message = error,
                Errors = new List<string> { error }
            };
        }
        
        // Begin transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Create new user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = DateTime.SpecifyKind(request.BirthDate, DateTimeKind.Utc),
                Gender = request.Gender,
                SexualPreference = request.SexualPreference,
                Biography = "",
                InterestTags = new List<string>(),
                ProfilePhotoUrl = "/images/default-avatar.png",
                PhotoUrls = new List<string>(),
                Latitude = 0,
                Longitude = 0,
                FameRating = 0,
                IsOnline = false,
                LastSeen = DateTime.UtcNow,
                IsEmailVerified = false,
                EmailVerifiedAt = null,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
            
            // Store password
            var passwordEntry = new UserPassword
            {
                UserId = user.Id,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.UserPasswords.Add(passwordEntry);
            await _context.SaveChangesAsync();
            
            // Create email verification token
            var verificationToken = GenerateSecureToken();
            var emailVerification = new EmailVerification
            {
                UserId = user.Id,
                Token = verificationToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };
            
            _context.EmailVerifications.Add(emailVerification);
            await _context.SaveChangesAsync();
            
            // Send verification email
            await _emailService.SendVerificationEmailAsync(user.Email, user.Username, verificationToken);
            
            await transaction.CommitAsync();
            
            return new AuthResult
            {
                Success = true,
                Message = "Registration successful! Please check your email to verify your account.",
                User = user
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Registration error: {ex}");
            return new AuthResult
            {
                Success = false,
                Message = "An error occurred during registration",
                Errors = new List<string> { "Registration failed. Please try again." }
            };
        }
    }
    
    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var errors = ValidateLogin(request);
        if (errors.Any())
        {
            return new AuthResult
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            };
        }
        
        // Find user by username or email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username);
            
        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password",
                Errors = new List<string> { "Invalid credentials" }
            };
        }
        
        // Check if email is verified
        if (!user.IsEmailVerified)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Email not verified",
                Errors = new List<string> { "Please verify your email before logging in" }
            };
        }
        
        // Get password hash
        var passwordEntry = await _context.UserPasswords
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
            
        if (passwordEntry == null)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password",
                Errors = new List<string> { "Invalid credentials" }
            };
        }
        
        // Verify password
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, passwordEntry.PasswordHash);
        
        if (!isValidPassword)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password",
                Errors = new List<string> { "Invalid credentials" }
            };
        }
        
        // Update last seen
        user.LastSeen = DateTime.UtcNow;
        user.IsOnline = true;
        await _context.SaveChangesAsync();
        
        return new AuthResult
        {
            Success = true,
            Message = "Login successful",
            User = user
        };
    }
    
    public async Task<bool> VerifyEmailAsync(string token)
    {
        var verification = await _context.EmailVerifications
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Token == token && !v.IsUsed);
            
        if (verification == null || verification.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }
        
        verification.IsUsed = true;
        verification.User.IsEmailVerified = true;
        verification.User.EmailVerifiedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> SendPasswordResetAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            // Don't reveal if email exists
            return true;
        }
        
        // Create reset token
        var resetToken = GenerateSecureToken();
        var passwordReset = new PasswordReset
        {
            UserId = user.Id,
            Token = resetToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };
        
        _context.PasswordResets.Add(passwordReset);
        await _context.SaveChangesAsync();
        
        // Send reset email
        await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetToken);
        
        return true;
    }
    
    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var reset = await _context.PasswordResets
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token && !r.IsUsed);
            
        if (reset == null || reset.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }
        
        // Get user password entry
        var passwordEntry = await _context.UserPasswords
            .FirstOrDefaultAsync(p => p.UserId == reset.UserId);
            
        if (passwordEntry == null)
        {
            return false;
        }
        
        // Update password
        passwordEntry.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
        reset.IsUsed = true;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task LogoutAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsOnline = false;
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    private string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
    
    private List<string> ValidateRegistration(RegisterRequest request)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Username))
            errors.Add("Username is required");
        else if (request.Username.Length < 3)
            errors.Add("Username must be at least 3 characters");
        else if (request.Username.Length > 50)
            errors.Add("Username must be less than 50 characters");
        else if (!System.Text.RegularExpressions.Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_]+$"))
            errors.Add("Username can only contain letters, numbers, and underscores");
            
        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(request.Email))
            errors.Add("Invalid email format");
            
        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required");
        else if (request.Password.Length < 6)
            errors.Add("Password must be at least 6 characters");
        else if (!IsStrongPassword(request.Password))
            errors.Add("Password must contain at least one uppercase letter, one lowercase letter, and one number");
            
        if (request.Password != request.ConfirmPassword)
            errors.Add("Passwords do not match");
            
        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors.Add("First name is required");
            
        if (string.IsNullOrWhiteSpace(request.LastName))
            errors.Add("Last name is required");
            
        if (request.BirthDate == default(DateTime))
            errors.Add("Birth date is required");
        else if (DateTime.Now.Year - request.BirthDate.Year < 18)
            errors.Add("You must be at least 18 years old");
            
        if (string.IsNullOrWhiteSpace(request.Gender))
            errors.Add("Gender is required");
            
        if (string.IsNullOrWhiteSpace(request.SexualPreference))
            errors.Add("Sexual preference is required");
            
        return errors;
    }
    
    private List<string> ValidateLogin(LoginRequest request)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Username))
            errors.Add("Username or email is required");
            
        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required");
            
        return errors;
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private bool IsStrongPassword(string password)
    {
        return password.Any(char.IsUpper) && 
               password.Any(char.IsLower) && 
               password.Any(char.IsDigit);
    }
    
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var userPassword = await _context.UserPasswords
            .FirstOrDefaultAsync(up => up.UserId == userId);
            
        if (userPassword == null)
        {
            return false;
        }
        
        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, userPassword.PasswordHash))
        {
            return false;
        }
        
        // Update to new password
        userPassword.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> VerifyUserPasswordAsync(int userId, string password)
    {
        var userPassword = await _context.UserPasswords
            .FirstOrDefaultAsync(up => up.UserId == userId);
            
        if (userPassword == null)
        {
            return false;
        }
        
        return BCrypt.Net.BCrypt.Verify(password, userPassword.PasswordHash);
    }
}