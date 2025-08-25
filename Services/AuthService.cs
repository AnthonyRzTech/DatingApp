using Microsoft.EntityFrameworkCore;
using WebMatcha.Data;
using WebMatcha.Models;
using BCrypt.Net;

namespace WebMatcha.Services;

public class AuthService : IAuthService
{
    private readonly MatchaDbContext _context;
    
    public AuthService(MatchaDbContext context)
    {
        _context = context;
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
            var error = existingUser.Username == request.Username ? "Username already exists" : "Email already exists";
            return new AuthResult
            {
                Success = false,
                Message = error,
                Errors = new List<string> { error }
            };
        }
        
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
            LastSeen = DateTime.UtcNow
        };
        
        // Hash password with BCrypt
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
        
        // Store password hash in a separate table or extend User model
        // For now, we'll add a PasswordHash property to User model
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // We need to store the password hash - let's create a separate table for this
        var passwordEntry = new UserPassword
        {
            UserId = user.Id,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.UserPasswords.Add(passwordEntry);
        await _context.SaveChangesAsync();
        
        return new AuthResult
        {
            Success = true,
            Message = "Registration successful",
            User = user
        };
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
        
        // Find user by username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);
            
        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password",
                Errors = new List<string> { "Invalid credentials" }
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
        
        // Verify password with BCrypt
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
    
    private List<string> ValidateRegistration(RegisterRequest request)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Username))
            errors.Add("Username is required");
        else if (request.Username.Length < 3)
            errors.Add("Username must be at least 3 characters");
        else if (request.Username.Length > 50)
            errors.Add("Username must be less than 50 characters");
            
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
            errors.Add("Username is required");
            
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
}