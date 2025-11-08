using Npgsql;
using Dapper;
using WebMatcha.Models;
using BCrypt.Net;
using System.Security.Cryptography;

namespace WebMatcha.Services;

/// <summary>
/// CompleteAuthService - Refactoré avec requêtes SQL manuelles
/// Service d'authentification complet avec email verification et password reset
/// </summary>
public class CompleteAuthService : IAuthService
{
    private readonly string _connectionString;
    private readonly IEmailService _emailService;

    // Common passwords that must be rejected (CRITICAL - subject requirement)
    private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "123456", "123456789", "12345678", "12345", "1234567", "password1",
        "abc123", "qwerty", "monkey", "1234567890", "letmein", "trustno1", "dragon",
        "baseball", "111111", "iloveyou", "master", "sunshine", "ashley", "bailey",
        "passw0rd", "shadow", "123123", "654321", "superman", "qazwsx", "michael",
        "football", "welcome", "jesus", "ninja", "mustang", "password123", "admin",
        "hello", "charlie", "696969", "hottie", "freedom", "aa123456", "princess",
        "qwertyuiop", "solo", "loveme", "whatever", "donald", "dragon", "michael",
        "starwars", "computer", "michelle", "jessica", "pepper", "1111", "zxcvbnm",
        "555555", "11111111", "131313", "freedom", "777777", "pass", "maggie",
        "jordan", "superman", "harley", "1234", "robert", "matthew", "cheese",
        "tigger", "princess", "maverick", "austin", "hockey", "yellow", "ranger",
        "secret", "andrew", "samsung", "test123", "jordan23", "killer", "fuckyou",
        "trustno1", "batman", "thomas", "hockey", "ranger", "daniel", "online",
        "letmein", "test", "qwerty123", "welcome", "Login", "admin123", "abc123"
    };

    public CompleteAuthService(IConfiguration configuration, IEmailService emailService)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
        _emailService = emailService;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        // Sanitize inputs to prevent XSS attacks (CRITICAL security requirement)
        request.Username = InputSanitizer.SanitizeUsername(request.Username);
        request.Email = InputSanitizer.SanitizeEmail(request.Email);
        request.FirstName = InputSanitizer.SanitizeText(request.FirstName);
        request.LastName = InputSanitizer.SanitizeText(request.LastName);

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

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Check if username or email already exists
            const string checkExistsSql = "SELECT COUNT(*) FROM users WHERE username = @Username OR email = @Email";
            var existsCount = await connection.ExecuteScalarAsync<int>(checkExistsSql, new { request.Username, request.Email }, transaction);

            if (existsCount > 0)
            {
                // Check which one exists
                const string checkUsernameSql = "SELECT COUNT(*) FROM users WHERE username = @Username";
                var usernameExists = await connection.ExecuteScalarAsync<int>(checkUsernameSql, new { request.Username }, transaction) > 0;

                var error = usernameExists ? "Username already exists" : "Email already exists";
                await transaction.RollbackAsync();
                return new AuthResult
                {
                    Success = false,
                    Message = error,
                    Errors = new List<string> { error }
                };
            }

            // Create new user
            const string insertUserSql = @"
                INSERT INTO users (
                    username, email, first_name, last_name, birth_date,
                    gender, sexual_preference, biography, interest_tags,
                    profile_photo_url, photo_urls, latitude, longitude,
                    fame_rating, is_online, last_seen, is_email_verified,
                    email_verified_at, is_active, created_at
                ) VALUES (
                    @Username, @Email, @FirstName, @LastName, @BirthDate,
                    @Gender, @SexualPreference, '', '',
                    '/images/default-avatar.png', '', 0, 0,
                    0, false, @Now, false,
                    NULL, true, @Now
                )
                RETURNING id
            ";

            var now = DateTime.UtcNow;
            var userId = await connection.ExecuteScalarAsync<int>(insertUserSql, new
            {
                request.Username,
                request.Email,
                request.FirstName,
                request.LastName,
                BirthDate = DateTime.SpecifyKind(request.BirthDate, DateTimeKind.Utc),
                request.Gender,
                request.SexualPreference,
                Now = now
            }, transaction);

            // Hash and store password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
            const string insertPasswordSql = @"
                INSERT INTO user_passwords (user_id, password_hash, created_at)
                VALUES (@UserId, @PasswordHash, @CreatedAt)
            ";
            await connection.ExecuteAsync(insertPasswordSql, new
            {
                UserId = userId,
                PasswordHash = hashedPassword,
                CreatedAt = now
            }, transaction);

            // Create email verification token
            var verificationToken = GenerateSecureToken();
            const string insertVerificationSql = @"
                INSERT INTO email_verifications (user_id, token, created_at, expires_at, is_used)
                VALUES (@UserId, @Token, @CreatedAt, @ExpiresAt, false)
            ";
            await connection.ExecuteAsync(insertVerificationSql, new
            {
                UserId = userId,
                Token = verificationToken,
                CreatedAt = now,
                ExpiresAt = now.AddHours(24)
            }, transaction);

            await transaction.CommitAsync();

            // Send verification email
            await _emailService.SendVerificationEmailAsync(request.Email, request.Username, verificationToken);

            // Get created user
            const string getUserSql = @"
                SELECT id, username, email, first_name AS FirstName, last_name AS LastName,
                    birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
                    biography, interest_tags AS InterestTags, profile_photo_url AS ProfilePhotoUrl,
                    photo_urls AS PhotoUrls, latitude, longitude, fame_rating AS FameRating,
                    is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
                    email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
                    created_at AS CreatedAt
                FROM users WHERE id = @UserId
            ";
            var user = await connection.QueryFirstAsync<User>(getUserSql, new { UserId = userId });

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

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Find user by username or email
        const string findUserSql = @"
            SELECT id, username, email, first_name AS FirstName, last_name AS LastName,
                birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
                biography, profile_photo_url AS ProfilePhotoUrl,
                latitude, longitude, fame_rating AS FameRating,
                is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
                email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
                created_at AS CreatedAt
            FROM users
            WHERE username = @Username OR email = @Username
        ";
        var user = await connection.QueryFirstOrDefaultAsync<User>(findUserSql, new { Username = request.Username });

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
        const string getPasswordSql = "SELECT password_hash FROM user_passwords WHERE user_id = @UserId";
        var passwordHash = await connection.QueryFirstOrDefaultAsync<string>(getPasswordSql, new { UserId = user.Id });

        if (string.IsNullOrEmpty(passwordHash))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password",
                Errors = new List<string> { "Invalid credentials" }
            };
        }

        // Verify password
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, passwordHash);

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
        const string updateLastSeenSql = "UPDATE users SET last_seen = @Now, is_online = true WHERE id = @UserId";
        await connection.ExecuteAsync(updateLastSeenSql, new { UserId = user.Id, Now = DateTime.UtcNow });

        user.LastSeen = DateTime.UtcNow;
        user.IsOnline = true;

        return new AuthResult
        {
            Success = true,
            Message = "Login successful",
            User = user
        };
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Find verification token
            const string findTokenSql = @"
                SELECT user_id, expires_at
                FROM email_verifications
                WHERE token = @Token AND is_used = false
            ";
            var verification = await connection.QueryFirstOrDefaultAsync<(int user_id, DateTime expires_at)>(findTokenSql, new { Token = token }, transaction);

            if (verification == default || verification.expires_at < DateTime.UtcNow)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Mark token as used
            const string markUsedSql = "UPDATE email_verifications SET is_used = true WHERE token = @Token";
            await connection.ExecuteAsync(markUsedSql, new { Token = token }, transaction);

            // Update user email verified status
            const string verifyUserSql = @"
                UPDATE users
                SET is_email_verified = true, email_verified_at = @Now
                WHERE id = @UserId
            ";
            await connection.ExecuteAsync(verifyUserSql, new { UserId = verification.user_id, Now = DateTime.UtcNow }, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> SendPasswordResetAsync(string email)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Find user by email
        const string findUserSql = "SELECT id, username FROM users WHERE email = @Email";
        var user = await connection.QueryFirstOrDefaultAsync<(int id, string username)>(findUserSql, new { Email = email });

        if (user == default)
        {
            // Don't reveal if email exists
            return true;
        }

        // Create reset token
        var resetToken = GenerateSecureToken();
        const string insertResetSql = @"
            INSERT INTO password_resets (user_id, token, created_at, expires_at, is_used)
            VALUES (@UserId, @Token, @CreatedAt, @ExpiresAt, false)
        ";
        var now = DateTime.UtcNow;
        await connection.ExecuteAsync(insertResetSql, new
        {
            UserId = user.id,
            Token = resetToken,
            CreatedAt = now,
            ExpiresAt = now.AddHours(1)
        });

        // Send reset email
        await _emailService.SendPasswordResetEmailAsync(email, user.username, resetToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Find reset token
            const string findTokenSql = @"
                SELECT user_id, expires_at
                FROM password_resets
                WHERE token = @Token AND is_used = false
            ";
            var reset = await connection.QueryFirstOrDefaultAsync<(int user_id, DateTime expires_at)>(findTokenSql, new { Token = token }, transaction);

            if (reset == default || reset.expires_at < DateTime.UtcNow)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Hash new password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);

            // Update password
            const string updatePasswordSql = "UPDATE user_passwords SET password_hash = @PasswordHash WHERE user_id = @UserId";
            var rowsAffected = await connection.ExecuteAsync(updatePasswordSql, new { UserId = reset.user_id, PasswordHash = hashedPassword }, transaction);

            if (rowsAffected == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // Mark token as used
            const string markUsedSql = "UPDATE password_resets SET is_used = true WHERE token = @Token";
            await connection.ExecuteAsync(markUsedSql, new { Token = token }, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task LogoutAsync(int userId)
    {
        const string sql = "UPDATE users SET is_online = false, last_seen = @Now WHERE id = @UserId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new { UserId = userId, Now = DateTime.UtcNow });
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Get current password hash
        const string getPasswordSql = "SELECT password_hash FROM user_passwords WHERE user_id = @UserId";
        var passwordHash = await connection.QueryFirstOrDefaultAsync<string>(getPasswordSql, new { UserId = userId });

        if (string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, passwordHash))
        {
            return false;
        }

        // Update to new password
        var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
        const string updatePasswordSql = "UPDATE user_passwords SET password_hash = @PasswordHash WHERE user_id = @UserId";
        await connection.ExecuteAsync(updatePasswordSql, new { UserId = userId, PasswordHash = newHashedPassword });

        return true;
    }

    public async Task<bool> VerifyUserPasswordAsync(int userId, string password)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string getPasswordSql = "SELECT password_hash FROM user_passwords WHERE user_id = @UserId";
        var passwordHash = await connection.QueryFirstOrDefaultAsync<string>(getPasswordSql, new { UserId = userId });

        if (string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    private string GenerateSecureToken()
    {
        // Generate cryptographically secure random token (256 bits)
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        // Add timestamp entropy for uniqueness
        var timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        var combined = new byte[randomBytes.Length + timestamp.Length];
        Buffer.BlockCopy(randomBytes, 0, combined, 0, randomBytes.Length);
        Buffer.BlockCopy(timestamp, 0, combined, randomBytes.Length, timestamp.Length);

        // Hash the combined data for additional security
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(combined);

        // Convert to URL-safe Base64
        return Convert.ToBase64String(hash)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private bool IsTokenExpired(DateTime expiresAt)
    {
        return DateTime.UtcNow > expiresAt;
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
        else if (CommonPasswords.Contains(request.Password))
            errors.Add("Password is too common. Please choose a more secure password");

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
}
