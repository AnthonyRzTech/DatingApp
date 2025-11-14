# CLAUDE.md - AI Assistant Guide for WebMatcha

**Last Updated:** 2025-11-14
**Project:** WebMatcha - Dating Application
**Framework:** ASP.NET Core 9.0 Blazor Server
**Database:** PostgreSQL 14+ with Manual SQL (Dapper)

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Quick Start for AI Assistants](#quick-start-for-ai-assistants)
3. [Tech Stack & Architecture](#tech-stack--architecture)
4. [Directory Structure](#directory-structure)
5. [Critical Conventions](#critical-conventions)
6. [Database Patterns](#database-patterns)
7. [Security Requirements](#security-requirements)
8. [Common Development Tasks](#common-development-tasks)
9. [Blazor Component Patterns](#blazor-component-patterns)
10. [Testing & Debugging](#testing--debugging)
11. [Pitfalls to Avoid](#pitfalls-to-avoid)
12. [Git Workflow](#git-workflow)

---

## Project Overview

### What is WebMatcha?

WebMatcha is a production-ready dating application built for École 42. It demonstrates:

- **Manual SQL proficiency** (100% parameterized queries with Dapper - **NO LINQ, NO EF CORE**)
- **Security-first architecture** (BCrypt, XSS protection, SQL injection prevention, CSRF tokens)
- **Real-time features** (SignalR for chat and notifications with <1s latency)
- **Performance optimization** (60+ PostgreSQL indexes, batch operations)
- **500+ auto-generated test profiles** with realistic interactions

### Critical Project Constraint

**MANDATORY:** All database operations MUST use manual SQL queries with Dapper. Entity Framework LINQ queries are prohibited per project requirements. Violations result in automatic failure.

### Build Status

```
✅ 0 Errors
⚠️  7 Warnings (async without await in Blazor - can be ignored)
```

---

## Quick Start for AI Assistants

### Before Making Changes

1. **Read the security section** - All code must follow security patterns
2. **Use parameterized queries ONLY** - String interpolation in SQL = automatic failure
3. **Test with demo user** - Username: `demo`, Password: `Demo123!`
4. **Check existing patterns** - Browse similar service files before writing new code
5. **Run the build** - `dotnet build` before committing

### Development Server

```bash
# Start with hot reload
dotnet watch run

# Access at:
https://localhost:7036  # Recommended (HTTPS)
http://localhost:5192   # Alternative (HTTP)
```

### Default Database Connection

If no `.env` file exists, defaults to:
```
Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q
```

---

## Tech Stack & Architecture

### Core Technologies

| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| **Framework** | ASP.NET Core Blazor Server | 9.0 | Web framework with server-side rendering |
| **Language** | C# | 12.0 | Primary language (implicit usings, nullable reference types) |
| **Database** | PostgreSQL | 14+ | Primary data store |
| **ORM** | Dapper | 2.1.66 | **Micro-ORM for manual SQL ONLY** |
| **Database Driver** | Npgsql | 9.0.4 | PostgreSQL .NET driver |
| **Real-time** | SignalR | 9.0 | WebSocket-based real-time communication |
| **Security** | BCrypt.Net-Next | 4.0.3 | Password hashing (workfactor 12) |
| **Email** | MailKit | 4.13.0 | SMTP email sending |
| **UI** | Bootstrap 5 | 5.3 | Responsive CSS framework |
| **Validation** | FluentValidation | 12.0.0 | Complex validation rules |
| **Environment** | DotNetEnv | 3.1.1 | .env file loading |

### Architecture Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                    Blazor Components                         │
│  (Components/Pages/*.razor, Components/Layout/*.razor)       │
└────────────────────┬────────────────────────────────────────┘
                     │ @inject
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                   Service Layer                              │
│  (Services/*.cs - All business logic + SQL queries)          │
└────────────────────┬────────────────────────────────────────┘
                     │ Dapper
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                PostgreSQL Database                           │
│  (11 tables, 60+ indexes, manual schema)                     │
└─────────────────────────────────────────────────────────────┘
```

**No repository pattern** - Services act as repositories with direct SQL access.

---

## Directory Structure

```
/home/user/DatingApp/
├── Components/                    # Blazor UI layer
│   ├── Layout/                   # MainLayout.razor, NavMenu.razor, AuthNavMenu.razor
│   ├── Pages/                    # 16 page components (Browse, Chat, Profile, etc.)
│   │   ├── Browse.razor          # User suggestions (age, distance, tags, orientation)
│   │   ├── Chat.razor            # Real-time messaging (SignalR)
│   │   ├── Login.razor           # Authentication
│   │   ├── Profile.razor         # View user profiles
│   │   ├── ProfileEdit.razor     # Edit own profile
│   │   ├── Register.razor        # User registration
│   │   ├── Search.razor          # Advanced search with filters
│   │   └── ...                   # (see section 9 for full list)
│   └── Shared/                   # Reusable components
│       └── AuthRequired.razor    # Authentication guard wrapper
│
├── Data/                         # [Empty - no DbContext used]
│
├── Hubs/                         # SignalR real-time hubs
│   └── ChatHub.cs                # Chat + presence management (148 lines)
│
├── Middleware/                   # Custom middleware
│   └── GlobalExceptionHandler.cs # Centralized error handling
│
├── Models/                       # DTOs and entities
│   ├── User.cs                   # User, Like, Match, Message, etc.
│   ├── Auth.cs                   # LoginRequest, RegisterRequest, AuthResult
│   ├── UserPassword.cs           # Password entities
│   └── EmailVerification.cs      # Email verification tokens
│
├── Services/                     # 🔥 BUSINESS LOGIC LAYER (ALL SQL MANUAL)
│   ├── CompleteAuthService.cs               # 572 lines - Auth + email verification
│   ├── UserService.cs                       # 526 lines - CRUD + search + suggestions
│   ├── MatchingService.cs                   # 444 lines - Likes, matches, transactions
│   ├── DataSeederService.cs                 # 305 lines - 500 profile generation
│   ├── MessageService.cs                    # 208 lines - Chat with SQL CTEs
│   ├── ServerSessionService.cs              # 200 lines - Session management
│   ├── DatabaseSchemaService.cs             # 225 lines - Schema creation (pure SQL)
│   ├── DatabaseOptimizationService.cs       # 120 lines - 60+ index creation
│   ├── BlockReportService.cs                # 147 lines - Block/report logic
│   ├── ProfileViewService.cs                # 114 lines - Profile views + fame
│   ├── NotificationService.cs               # 125 lines - Real-time notifications
│   ├── PhotoService.cs                      # 135 lines - Upload validation
│   ├── InputSanitizer.cs                    # 140 lines - XSS protection
│   ├── EmailService.cs                      # 183 lines - Email sending
│   ├── DapperTypeHandlers.cs                # 48 lines - Custom type handlers
│   └── [3 more legacy services]
│
├── SQL/                          # SQL scripts
│   ├── AddIndexes.sql            # 60+ PostgreSQL index definitions
│   └── InsertTestUser.sql        # Demo user creation
│
├── Tests/                        # Integration tests
│   └── LoginIntegrationTest.cs
│
├── wwwroot/                      # Static files + uploads
│   ├── js/                       # JavaScript (geolocation.js)
│   ├── lib/                      # Bootstrap 5.3
│   └── uploads/                  # User photo uploads
│       └── photos/               # Profile photos
│
├── Program.cs                    # 🔥 Entry point, DI config, endpoints, startup
├── WebMatcha.csproj              # Project dependencies
├── WebMatcha.sln                 # Solution file
├── appsettings.json              # Configuration
├── .env                          # Environment variables (connection string)
├── README.md                     # Installation guide (French)
├── PROJECT_STATUS.md             # Current status & checklist
├── CHANGELOG.md                  # Version history
├── subject.md                    # School project requirements
└── CLAUDE.md                     # This file
```

---

## Critical Conventions

### Naming Conventions

| Layer | Convention | Example |
|-------|------------|---------|
| **PostgreSQL Tables** | snake_case | `users`, `profile_views`, `email_verifications` |
| **PostgreSQL Columns** | snake_case | `first_name`, `is_email_verified`, `created_at` |
| **C# Classes** | PascalCase | `User`, `MatchingService`, `AuthResult` |
| **C# Properties** | PascalCase | `FirstName`, `IsEmailVerified`, `CreatedAt` |
| **C# Methods** | PascalCase + Async suffix | `GetUserByIdAsync()`, `SendMessageAsync()` |
| **SQL Parameters** | PascalCase with @ | `@UserId`, `@Username`, `@Latitude` |
| **Private Fields** | _camelCase | `_connectionString`, `_logger`, `_userConnections` |
| **Blazor Components** | PascalCase.razor | `Browse.razor`, `AuthRequired.razor` |

### Special Database Naming Notes

⚠️ **Important:** Some table columns use inconsistent naming (legacy from migration):

- `matches` table: Uses `user1id`, `user2id` (NOT `user1_id`, `user2_id`)
- Most other tables: Standard `snake_case` with underscores

When querying, always verify actual column names in `DatabaseSchemaService.cs`.

### C# Conventions

```csharp
// File: Services/UserService.cs

public class UserService  // PascalCase class name
{
    private readonly string _connectionString;  // _camelCase for private fields
    private readonly ILogger<UserService> _logger;

    // Constructor injection (DI)
    public UserService(IConfiguration configuration, ILogger<UserService> logger)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
        _logger = logger;
    }

    // PascalCase method name + Async suffix
    public async Task<User?> GetUserByIdAsync(int id)  // Nullable return type
    {
        // const for SQL queries (enables string pooling)
        const string sql = @"
            SELECT
                id, username, email,
                first_name AS FirstName,  -- Map snake_case to PascalCase
                last_name AS LastName,
                is_active AS IsActive
            FROM users
            WHERE id = @Id  -- PascalCase parameter
        ";

        using var connection = new NpgsqlConnection(_connectionString);  // using for disposal
        await connection.OpenAsync();  // Explicit async open

        var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });

        return user;
    }
}
```

---

## Database Patterns

### Schema Definition

All tables are defined in `Services/DatabaseSchemaService.cs` using pure SQL DDL:

```csharp
private async Task CreateUsersTableAsync(NpgsqlConnection connection)
{
    const string sql = @"
        CREATE TABLE IF NOT EXISTS users (
            id SERIAL PRIMARY KEY,
            username VARCHAR(50) UNIQUE NOT NULL,
            email VARCHAR(255) UNIQUE NOT NULL,
            first_name VARCHAR(100) NOT NULL,
            last_name VARCHAR(100) NOT NULL,
            birth_date DATE NOT NULL,
            gender VARCHAR(20) NOT NULL,
            sexual_preference VARCHAR(20) NOT NULL,
            biography TEXT,
            interest_tags TEXT,  -- Comma-separated
            profile_photo_url TEXT,
            photo_urls TEXT,  -- Comma-separated
            latitude DOUBLE PRECISION,
            longitude DOUBLE PRECISION,
            fame_rating INTEGER DEFAULT 0,
            is_online BOOLEAN DEFAULT FALSE,
            last_seen TIMESTAMP WITH TIME ZONE,
            is_email_verified BOOLEAN DEFAULT FALSE,
            email_verified_at TIMESTAMP WITH TIME ZONE,
            is_active BOOLEAN DEFAULT TRUE,
            deactivated_at TIMESTAMP WITH TIME ZONE,
            created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
        );
    ";

    await connection.ExecuteAsync(sql);
}
```

### 11 Database Tables

1. **users** - Main user profiles
2. **user_passwords** - Password hashes (separate for security)
3. **likes** - Like relationships (liker_id, liked_id)
4. **matches** - Mutual likes (user1id, user2id)
5. **messages** - Chat messages
6. **notifications** - Real-time notifications
7. **profile_views** - Profile view history
8. **blocks** - Blocked users
9. **reports** - Fake account reports
10. **email_verifications** - Email verification tokens
11. **password_resets** - Password reset tokens

### SQL Query Patterns

#### ✅ CORRECT - Parameterized Query (ALWAYS USE THIS)

```csharp
const string sql = @"
    SELECT * FROM users
    WHERE username = @Username
      AND is_active = @IsActive
";

using var connection = new NpgsqlConnection(_connectionString);
var user = await connection.QueryFirstOrDefaultAsync<User>(
    sql,
    new { Username = username, IsActive = true }  // Anonymous object for parameters
);
```

#### ❌ FORBIDDEN - String Interpolation (NEVER USE THIS)

```csharp
// ❌ SQL INJECTION VULNERABILITY - WILL FAIL PROJECT
var sql = $"SELECT * FROM users WHERE username = '{username}'";

// ❌ ALSO FORBIDDEN - String concatenation
var sql = "SELECT * FROM users WHERE username = '" + username + "'";
```

### Connection Management Pattern

**Standard pattern** (used in 90% of services):

```csharp
using var connection = new NpgsqlConnection(_connectionString);
await connection.OpenAsync();  // Optional - Dapper opens automatically if needed

var result = await connection.QueryAsync<User>(sql, parameters);
// Connection automatically disposed at end of using block
```

### Transaction Pattern

**Use for multi-step operations** (e.g., like → check reciprocal → create match):

```csharp
using var connection = new NpgsqlConnection(_connectionString);
await connection.OpenAsync();
using var transaction = await connection.BeginTransactionAsync();

try
{
    // Step 1: Insert like
    const string insertLikeSql = "INSERT INTO likes (liker_id, liked_id) VALUES (@LikerId, @LikedId)";
    await connection.ExecuteAsync(insertLikeSql, new { LikerId = likerId, LikedId = likedId }, transaction);

    // Step 2: Check for reciprocal like
    const string checkSql = @"
        SELECT COUNT(*) FROM likes
        WHERE liker_id = @LikedId AND liked_id = @LikerId
    ";
    var hasReciprocal = await connection.ExecuteScalarAsync<int>(checkSql, new { LikerId = likerId, LikedId = likedId }, transaction);

    // Step 3: Create match if reciprocal
    if (hasReciprocal > 0)
    {
        const string createMatchSql = @"
            INSERT INTO matches (user1id, user2id)
            VALUES (LEAST(@User1, @User2), GREATEST(@User1, @User2))
        ";
        await connection.ExecuteAsync(createMatchSql, new { User1 = likerId, User2 = likedId }, transaction);
    }

    await transaction.CommitAsync();  // All or nothing
}
catch
{
    await transaction.RollbackAsync();  // Rollback on any error
    throw;
}
```

### Dapper Mapping: snake_case → PascalCase

**Always use AS aliases** to map PostgreSQL column names to C# properties:

```csharp
const string sql = @"
    SELECT
        id,                              -- Matches C# property: Id
        username,                        -- Matches C# property: Username
        first_name AS FirstName,         -- Maps first_name → FirstName
        last_name AS LastName,           -- Maps last_name → LastName
        is_email_verified AS IsEmailVerified,  -- Maps is_email_verified → IsEmailVerified
        created_at AS CreatedAt          -- Maps created_at → CreatedAt
    FROM users
    WHERE id = @Id
";
```

### Custom Type Handlers

**Comma-separated strings → List&lt;string&gt;** (see `DapperTypeHandlers.cs`):

```csharp
// Database stores: "hiking,reading,music"
// C# receives: List<string> { "hiking", "reading", "music" }

// Configured in Program.cs:
SqlMapper.AddTypeHandler(new CommaSeparatedStringListTypeHandler());

// Usage in models:
public class User
{
    public List<string> InterestTags { get; set; } = new();  // Automatically converted
    public List<string> PhotoUrls { get; set; } = new();     // Automatically converted
}
```

### Complex Query Examples

#### Haversine Distance Calculation

```csharp
// UserService.cs - Get nearby users sorted by distance
const string sql = @"
    WITH blocked_users AS (
        SELECT CASE WHEN blocker_id = @UserId THEN blocked_id ELSE blocker_id END AS user_id
        FROM blocks WHERE blocker_id = @UserId OR blocked_id = @UserId
    )
    SELECT u.*,
        (6371 * acos(
            cos(radians(@Latitude)) * cos(radians(u.latitude)) *
            cos(radians(u.longitude) - radians(@Longitude)) +
            sin(radians(@Latitude)) * sin(radians(u.latitude))
        )) AS Distance
    FROM users u
    WHERE u.id != @UserId
      AND u.id NOT IN (SELECT user_id FROM blocked_users)
      AND u.is_active = TRUE
    ORDER BY Distance ASC, u.fame_rating DESC
    LIMIT @Limit
";
```

#### SQL CTE for Conversation History

```csharp
// MessageService.cs - Get last N messages in conversation
const string sql = @"
    SELECT * FROM (
        SELECT
            id, sender_id AS SenderId, receiver_id AS ReceiverId,
            content, sent_at AS SentAt, is_read AS IsRead
        FROM messages
        WHERE (sender_id = @UserId AND receiver_id = @OtherUserId)
           OR (sender_id = @OtherUserId AND receiver_id = @UserId)
        ORDER BY sent_at DESC
        LIMIT @Count
    ) AS recent_messages
    ORDER BY sent_at ASC  -- Oldest first for display
";
```

#### Batch Insert Pattern

```csharp
// DataSeederService.cs - Insert 500 users efficiently
const string sql = @"
    INSERT INTO users (username, email, first_name, last_name, birth_date, gender, ...)
    VALUES (@Username, @Email, @FirstName, @LastName, @BirthDate, @Gender, ...)
";

using var connection = new NpgsqlConnection(_connectionString);
await connection.OpenAsync();

foreach (var batch in users.Chunk(100))  // Insert in batches of 100
{
    await connection.ExecuteAsync(sql, batch);  // Dapper handles batch automatically
}
```

---

## Security Requirements

**CRITICAL:** Security violations result in automatic project failure (0%).

### 1. Password Security

#### BCrypt Hashing

```csharp
// CompleteAuthService.cs - ALWAYS use BCrypt workfactor 12
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, 12);

// Verification
var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
```

#### Common Password Rejection

```csharp
// CompleteAuthService.cs lines 19-35
private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
{
    "password", "123456", "123456789", "qwerty", "abc123", "letmein",
    "welcome", "monkey", "dragon", "master", "sunshine", "princess",
    // ... 100+ common passwords
};

// Validation
if (CommonPasswords.Contains(password))
{
    return new AuthResult { Success = false, Error = "Password is too common" };
}
```

### 2. SQL Injection Protection

**MANDATORY:** All queries MUST use parameterized queries.

```csharp
// ✅ CORRECT
const string sql = "SELECT * FROM users WHERE username = @Username";
await connection.QueryAsync<User>(sql, new { Username = username });

// ❌ FORBIDDEN - WILL FAIL PROJECT
var sql = $"SELECT * FROM users WHERE username = '{username}'";
```

### 3. XSS Protection

```csharp
// InputSanitizer.cs - Use for ALL user-generated content
public static string SanitizeText(string? input)
{
    if (string.IsNullOrWhiteSpace(input)) return string.Empty;

    var sanitized = input;

    // Remove dangerous patterns
    sanitized = ScriptTagPattern.Replace(sanitized, string.Empty);        // <script>
    sanitized = OnEventPattern.Replace(sanitized, string.Empty);          // onclick=
    sanitized = JavascriptPattern.Replace(sanitized, string.Empty);       // javascript:
    sanitized = DataAttributePattern.Replace(sanitized, string.Empty);    // data:text/html

    // HTML encode
    sanitized = WebUtility.HtmlEncode(sanitized);

    return sanitized.Trim();
}

// Usage in services
var sanitizedBio = InputSanitizer.SanitizeText(biography);
```

### 4. CSRF Protection

Configured in `Program.cs`:

```csharp
// Session cookies (lines 58-66)
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;       // Prevent JavaScript access
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;  // CSRF protection
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS only
});

// Enable antiforgery (line 118)
app.UseAntiforgery();
```

### 5. File Upload Validation

```csharp
// PhotoService.cs - MIME type + magic number validation
private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
private readonly long _maxFileSize = 5 * 1024 * 1024;  // 5MB

private static readonly Dictionary<string, byte[][]> ImageSignatures = new()
{
    { "image/jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
    { "image/png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
    // ...
};

public async Task<UploadResult> UploadPhotoAsync(IFormFile file)
{
    // 1. Check MIME type
    if (!_allowedMimeTypes.Contains(file.ContentType))
        return new UploadResult { Success = false, Error = "Invalid file type" };

    // 2. Check file size
    if (file.Length > _maxFileSize)
        return new UploadResult { Success = false, Error = "File too large" };

    // 3. Verify magic numbers
    using var stream = file.OpenReadStream();
    var header = new byte[8];
    await stream.ReadAsync(header, 0, 8);

    if (!IsValidImageSignature(file.ContentType, header))
        return new UploadResult { Success = false, Error = "File signature mismatch" };

    // ... proceed with upload
}
```

### 6. Session Hijacking Prevention

```csharp
// ServerSessionService.cs - Validate IP + User-Agent
private bool ValidateSession()
{
    var currentIp = _httpContext.Connection.RemoteIpAddress?.ToString();
    var currentUserAgent = _httpContext.Request.Headers["User-Agent"].ToString();

    var sessionIp = _httpContext.Session.GetString("SessionIp");
    var sessionUserAgent = _httpContext.Session.GetString("SessionUserAgent");

    if (sessionIp != currentIp || sessionUserAgent != currentUserAgent)
    {
        // Session hijack detected
        _httpContext.Session.Clear();
        return false;
    }

    return true;
}
```

### 7. Secure Token Generation

```csharp
// CompleteAuthService.cs - Email verification & password reset tokens
private string GenerateSecureToken()
{
    using var rng = RandomNumberGenerator.Create();
    var bytes = new byte[32];  // 256 bits of entropy
    rng.GetBytes(bytes);

    using var sha256 = SHA256.Create();
    var hash = sha256.ComputeHash(bytes);

    return Convert.ToBase64String(hash)
        .Replace("+", "-")
        .Replace("/", "_")
        .TrimEnd('=');
}
```

### 8. Security Headers

```csharp
// Program.cs lines 96-112
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(self), camera=(), microphone=()");

    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' wss: https:;");
    }

    await next();
});
```

---

## Common Development Tasks

### Task 1: Add a New Service

**Example:** Creating a `FriendRequestService`

#### Step 1: Create the service file

```csharp
// File: Services/FriendRequestService.cs
using Dapper;
using Npgsql;

namespace WebMatcha.Services;

public class FriendRequestService
{
    private readonly string _connectionString;
    private readonly ILogger<FriendRequestService> _logger;

    public FriendRequestService(IConfiguration configuration, ILogger<FriendRequestService> logger)
    {
        _connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=q";
        _logger = logger;
    }

    public async Task<bool> SendFriendRequestAsync(int senderId, int receiverId)
    {
        const string sql = @"
            INSERT INTO friend_requests (sender_id, receiver_id, status, created_at)
            VALUES (@SenderId, @ReceiverId, 'pending', NOW())
            ON CONFLICT (sender_id, receiver_id) DO NOTHING
        ";

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(sql, new { SenderId = senderId, ReceiverId = receiverId });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request from {SenderId} to {ReceiverId}", senderId, receiverId);
            return false;
        }
    }
}
```

#### Step 2: Register in Program.cs

```csharp
// Program.cs - Add to service registration section (around line 36-52)
builder.Services.AddScoped<FriendRequestService>();
```

#### Step 3: Add database table

```csharp
// Services/DatabaseSchemaService.cs - Add new method
private async Task CreateFriendRequestsTableAsync(NpgsqlConnection connection)
{
    const string sql = @"
        CREATE TABLE IF NOT EXISTS friend_requests (
            id SERIAL PRIMARY KEY,
            sender_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            receiver_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            status VARCHAR(20) NOT NULL DEFAULT 'pending',
            created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
            UNIQUE(sender_id, receiver_id)
        );
    ";

    await connection.ExecuteAsync(sql);
}

// Add to EnsureDatabaseSchemaAsync() method
await CreateFriendRequestsTableAsync(connection);
```

#### Step 4: Add indexes

```csharp
// Services/DatabaseOptimizationService.cs - Add to ApplyOptimizationsAsync()
await ExecuteIndexAsync(connection, "idx_friend_requests_sender", "friend_requests(sender_id)");
await ExecuteIndexAsync(connection, "idx_friend_requests_receiver", "friend_requests(receiver_id)");
await ExecuteIndexAsync(connection, "idx_friend_requests_status", "friend_requests(status)");
```

#### Step 5: Use in Blazor component

```razor
@page "/friends"
@inject FriendRequestService FriendRequestService
@rendermode InteractiveServer

<h3>Send Friend Request</h3>

<button @onclick="() => SendRequest(userId)">Send Request</button>

@code {
    [Parameter]
    public int userId { get; set; }

    private async Task SendRequest(int receiverId)
    {
        var success = await FriendRequestService.SendFriendRequestAsync(currentUserId, receiverId);
        if (success)
        {
            // Show success message
        }
    }
}
```

### Task 2: Add a New Database Table

See Task 1, Step 3 above.

### Task 3: Add a New API Endpoint

```csharp
// Program.cs - Add after other endpoints (around line 200+)
app.MapPost("/api/friend-requests/send", async (HttpContext context, FriendRequestService service) =>
{
    var form = await context.Request.ReadFormAsync();
    var senderId = int.Parse(form["senderId"]!);
    var receiverId = int.Parse(form["receiverId"]!);

    var success = await service.SendFriendRequestAsync(senderId, receiverId);

    return success
        ? Results.Ok(new { success = true, message = "Friend request sent" })
        : Results.BadRequest(new { success = false, error = "Failed to send request" });
});
```

### Task 4: Add SignalR Real-Time Feature

```csharp
// Hubs/ChatHub.cs - Add new method
public async Task SendFriendRequest(int senderId, int receiverId)
{
    // Save to database
    var success = await _friendRequestService.SendFriendRequestAsync(senderId, receiverId);

    if (success)
    {
        // Notify receiver if online
        if (_userConnections.TryGetValue(receiverId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveFriendRequest", new
            {
                senderId,
                senderName = "John Doe",
                timestamp = DateTime.UtcNow
            });
        }

        // Confirm to sender
        await Clients.Caller.SendAsync("FriendRequestSent", new { receiverId });
    }
}
```

### Task 5: Add Complex Search Query

```csharp
public async Task<List<User>> SearchUsersAsync(SearchFilters filters)
{
    // Build dynamic WHERE clause (still parameterized!)
    var whereClauses = new List<string> { "u.is_active = TRUE" };
    var parameters = new DynamicParameters();

    if (filters.MinAge.HasValue)
    {
        whereClauses.Add("EXTRACT(YEAR FROM AGE(u.birth_date)) >= @MinAge");
        parameters.Add("MinAge", filters.MinAge.Value);
    }

    if (filters.MaxAge.HasValue)
    {
        whereClauses.Add("EXTRACT(YEAR FROM AGE(u.birth_date)) <= @MaxAge");
        parameters.Add("MaxAge", filters.MaxAge.Value);
    }

    if (!string.IsNullOrWhiteSpace(filters.Gender))
    {
        whereClauses.Add("u.gender = @Gender");
        parameters.Add("Gender", filters.Gender);
    }

    var whereClause = string.Join(" AND ", whereClauses);

    var sql = $@"
        SELECT u.*
        FROM users u
        WHERE {whereClause}
        ORDER BY u.fame_rating DESC
        LIMIT @Limit
    ";

    parameters.Add("Limit", filters.Limit ?? 50);

    using var connection = new NpgsqlConnection(_connectionString);
    var users = await connection.QueryAsync<User>(sql, parameters);
    return users.ToList();
}
```

**Note:** While the SQL string contains `{whereClause}`, the actual filter values are still parameterized (e.g., `@MinAge`, `@Gender`), preventing SQL injection.

---

## Blazor Component Patterns

### Component Structure

```razor
@* File: Components/Pages/Browse.razor *@
@page "/browse"
@using WebMatcha.Services
@using WebMatcha.Models
@inject UserService UserService
@inject MatchingService MatchingService
@inject ServerSessionService SessionService
@inject NavigationManager Navigation
@rendermode InteractiveServer

<PageTitle>Browse Users</PageTitle>

<AuthRequired>
    <div class="container mt-4">
        <h2>Browse Users</h2>

        @if (isLoading)
        {
            <p>Loading...</p>
        }
        else if (users.Count == 0)
        {
            <p>No users found.</p>
        }
        else
        {
            <div class="row">
                @foreach (var user in users)
                {
                    <div class="col-md-4 mb-4">
                        <div class="card">
                            <img src="@user.ProfilePhotoUrl" class="card-img-top" alt="@user.FirstName">
                            <div class="card-body">
                                <h5 class="card-title">@user.FirstName, @user.Age</h5>
                                <p class="card-text">@user.Biography</p>
                                <button class="btn btn-primary" @onclick="() => LikeUser(user.Id)">
                                    <i class="bi bi-heart"></i> Like
                                </button>
                            </div>
                        </div>
                    </div>
                }
            </div>
        }
    </div>
</AuthRequired>

@code {
    private List<User> users = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        var currentUser = await SessionService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        users = await UserService.GetSuggestionsAsync(currentUser.Id, 50);
        isLoading = false;
    }

    private async Task LikeUser(int userId)
    {
        var currentUser = await SessionService.GetCurrentUserAsync();
        if (currentUser == null) return;

        var result = await MatchingService.LikeUserAsync(currentUser.Id, userId);

        if (result.IsMatch)
        {
            // Show match notification
            await InvokeAsync(StateHasChanged);
        }
    }
}
```

### Page Components (16 total)

| Component | Route | Purpose | Auth Required |
|-----------|-------|---------|---------------|
| **Home.razor** | `/` | Landing page | No |
| **Register.razor** | `/register` | User registration | No |
| **Login.razor** | `/login` | User login | No |
| **VerifyEmail.razor** | `/verify-email` | Email verification | No |
| **ForgotPassword.razor** | `/forgot-password` | Password reset request | No |
| **ResetPassword.razor** | `/reset-password` | Password reset | No |
| **Profile.razor** | `/profile/{id}` | View user profile | Yes |
| **ProfileEdit.razor** | `/profile/edit` | Edit own profile | Yes |
| **Browse.razor** | `/browse` | Browse suggestions | Yes |
| **Search.razor** | `/search` | Advanced search | Yes |
| **Matches.razor** | `/matches` | View matches | Yes |
| **Chat.razor** | `/chat` | Real-time messaging | Yes |
| **Notifications.razor** | `/notifications` | Notification center | Yes |
| **AccountSettings.razor** | `/settings` | Account settings | Yes |
| **TestLogin.razor** | `/test-login` | Development login | No |
| **Error.razor** | `/error` | Error page | No |

### Authentication Guard Pattern

```razor
@* Shared/AuthRequired.razor *@
@inject ServerSessionService SessionService
@inject NavigationManager Navigation

@if (isAuthenticated)
{
    @ChildContent
}
else
{
    <p>Redirecting to login...</p>
}

@code {
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool isAuthenticated = false;

    protected override async Task OnInitializedAsync()
    {
        var user = await SessionService.GetCurrentUserAsync();
        isAuthenticated = user != null;

        if (!isAuthenticated)
        {
            Navigation.NavigateTo("/login");
        }
    }
}
```

### Service Injection Pattern

```razor
@* Always inject at top of file *@
@inject UserService UserService
@inject MatchingService MatchingService
@inject ServerSessionService SessionService
@inject NavigationManager Navigation
@inject IJSRuntime JS

@* Set render mode for interactive components *@
@rendermode InteractiveServer
```

### Event Handling

```razor
@* Button click *@
<button @onclick="HandleClick">Click Me</button>

@* Button click with parameter *@
<button @onclick="() => HandleClick(userId)">Click Me</button>

@* Input change *@
<input type="text" @bind="username" @bind:event="oninput" />

@* Select change *@
<select @onchange="OnSelectChange">
    <option value="1">Option 1</option>
</select>

@code {
    private string username = "";

    private async Task HandleClick()
    {
        // Handle click
    }

    private async Task HandleClick(int id)
    {
        // Handle click with parameter
    }

    private void OnSelectChange(ChangeEventArgs e)
    {
        var selectedValue = e.Value?.ToString();
    }
}
```

### JavaScript Interop

```razor
@inject IJSRuntime JS

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Call JavaScript function
            await JS.InvokeVoidAsync("console.log", "Component rendered");

            // Get return value from JavaScript
            var result = await JS.InvokeAsync<string>("localStorage.getItem", "key");
        }
    }
}
```

---

## Testing & Debugging

### Test Credentials

```
Username: demo
Password: Demo123!
Email: Already verified and active
```

### Development Server

```bash
# Start with hot reload
dotnet watch run

# Access at:
https://localhost:7036  # Recommended (HTTPS)
http://localhost:5192   # Alternative (HTTP)
```

### Check Database State

```bash
# Connect to PostgreSQL
psql -U postgres -d webmatcha

# Check user count
SELECT COUNT(*) FROM users;

# Check if demo user exists
SELECT * FROM users WHERE username = 'demo';

# Check matches
SELECT * FROM matches LIMIT 10;

# Check messages
SELECT * FROM messages ORDER BY sent_at DESC LIMIT 10;
```

### Common Issues & Solutions

#### Issue: "Database does not exist"

```bash
createdb -U postgres webmatcha
```

#### Issue: "Connection refused"

```bash
# Check if PostgreSQL is running
sudo service postgresql status

# Start PostgreSQL
sudo service postgresql start
```

#### Issue: "Password authentication failed"

Create `.env` file:

```env
CONNECTION_STRING=Host=localhost;Port=5432;Database=webmatcha;Username=postgres;Password=YOUR_PASSWORD
```

#### Issue: "No users generated"

Check logs for errors, then manually seed:

```bash
curl http://localhost:5192/api/seed
```

#### Issue: Build warnings about async methods

These are **non-blocking warnings** in Blazor components. Safe to ignore.

### Logging

All services log to console. Check startup logs for:

```
[DatabaseSchemaService] Creating database schema...
[DatabaseSchemaService] Database schema created successfully.
[DatabaseOptimizationService] Applying database optimizations...
[DatabaseOptimizationService] Created 60+ indexes.
[DataSeederService] Checking user count...
[DataSeederService] Database seeding completed. Generated 500 users.
```

### API Endpoints for Testing

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/health` | GET | Health check |
| `/api/users/count` | GET | Get user count |
| `/api/seed` | GET | Generate 500 profiles |
| `/api/debug/session` | GET | Check current session |

---

## Pitfalls to Avoid

### 1. ❌ Using String Interpolation in SQL

```csharp
// ❌ FORBIDDEN - SQL injection vulnerability
var sql = $"SELECT * FROM users WHERE username = '{username}'";

// ✅ CORRECT - Parameterized query
const string sql = "SELECT * FROM users WHERE username = @Username";
await connection.QueryAsync<User>(sql, new { Username = username });
```

### 2. ❌ Forgetting to Map snake_case to PascalCase

```csharp
// ❌ WRONG - Will return null for FirstName
const string sql = "SELECT first_name FROM users WHERE id = @Id";
var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
// user.FirstName will be null!

// ✅ CORRECT - Use AS alias
const string sql = "SELECT first_name AS FirstName FROM users WHERE id = @Id";
var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
// user.FirstName populated correctly
```

### 3. ❌ Not Sanitizing User Input

```csharp
// ❌ WRONG - XSS vulnerability
var biography = form["biography"];
await connection.ExecuteAsync(sql, new { Biography = biography });

// ✅ CORRECT - Sanitize first
var biography = InputSanitizer.SanitizeText(form["biography"]);
await connection.ExecuteAsync(sql, new { Biography = biography });
```

### 4. ❌ Not Using Transactions for Multi-Step Operations

```csharp
// ❌ WRONG - Race condition possible
await connection.ExecuteAsync("INSERT INTO likes ...");
var hasReciprocal = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM likes ...");
if (hasReciprocal > 0)
{
    await connection.ExecuteAsync("INSERT INTO matches ...");  // Might fail, leaving orphan like
}

// ✅ CORRECT - Use transaction
using var transaction = await connection.BeginTransactionAsync();
try
{
    await connection.ExecuteAsync("INSERT INTO likes ...", transaction: transaction);
    var hasReciprocal = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM likes ...", transaction: transaction);
    if (hasReciprocal > 0)
    {
        await connection.ExecuteAsync("INSERT INTO matches ...", transaction: transaction);
    }
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 5. ❌ Hardcoding Passwords

```csharp
// ❌ WRONG - Plain text password
const string sql = "INSERT INTO user_passwords (user_id, password_hash) VALUES (@UserId, @Password)";
await connection.ExecuteAsync(sql, new { UserId = userId, Password = "password123" });

// ✅ CORRECT - BCrypt hashing
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, 12);
const string sql = "INSERT INTO user_passwords (user_id, password_hash) VALUES (@UserId, @PasswordHash)";
await connection.ExecuteAsync(sql, new { UserId = userId, PasswordHash = hashedPassword });
```

### 6. ❌ Not Disposing Connections

```csharp
// ❌ WRONG - Connection leak
var connection = new NpgsqlConnection(_connectionString);
var users = await connection.QueryAsync<User>(sql);
// Connection never closed!

// ✅ CORRECT - Using statement ensures disposal
using var connection = new NpgsqlConnection(_connectionString);
var users = await connection.QueryAsync<User>(sql);
// Connection automatically closed and disposed
```

### 7. ❌ Using Entity Framework or LINQ to SQL

```csharp
// ❌ FORBIDDEN - Will fail project
var users = await _dbContext.Users
    .Where(u => u.IsActive)
    .OrderBy(u => u.FameRating)
    .ToListAsync();

// ✅ CORRECT - Manual SQL
const string sql = @"
    SELECT * FROM users
    WHERE is_active = TRUE
    ORDER BY fame_rating DESC
";
using var connection = new NpgsqlConnection(_connectionString);
var users = await connection.QueryAsync<User>(sql);
```

### 8. ❌ Forgetting @rendermode InteractiveServer

```razor
@* ❌ WRONG - Component won't be interactive *@
@page "/browse"
@inject UserService UserService

<button @onclick="LoadUsers">Load</button>  @* Won't work! *@

@* ✅ CORRECT - Add render mode *@
@page "/browse"
@inject UserService UserService
@rendermode InteractiveServer

<button @onclick="LoadUsers">Load</button>  @* Works! *@
```

### 9. ❌ Not Validating File Uploads

```csharp
// ❌ WRONG - Trust client-provided MIME type
if (file.ContentType == "image/jpeg")
{
    await SaveFileAsync(file);  // User can fake MIME type!
}

// ✅ CORRECT - Verify magic numbers
var header = new byte[8];
await file.OpenReadStream().ReadAsync(header);
if (!IsValidImageSignature(file.ContentType, header))
{
    throw new InvalidOperationException("Invalid file signature");
}
```

### 10. ❌ Not Checking User Authorization

```csharp
// ❌ WRONG - Any user can delete any photo
public async Task DeletePhotoAsync(int photoId)
{
    await connection.ExecuteAsync("DELETE FROM photos WHERE id = @Id", new { Id = photoId });
}

// ✅ CORRECT - Verify ownership
public async Task DeletePhotoAsync(int photoId, int userId)
{
    const string sql = "DELETE FROM photos WHERE id = @Id AND user_id = @UserId";
    var rowsAffected = await connection.ExecuteAsync(sql, new { Id = photoId, UserId = userId });

    if (rowsAffected == 0)
    {
        throw new UnauthorizedAccessException("You can only delete your own photos");
    }
}
```

---

## Git Workflow

### Current Branch

You are working on: `claude/claude-md-mhza4i3nh0ixg07a-012Uos5coK1Tt9eHxJD3EbgA`

### Committing Changes

```bash
# Stage files
git add Services/NewService.cs
git add Program.cs

# Commit with descriptive message
git commit -m "Add FriendRequestService with manual SQL queries

- Create FriendRequestService for sending/accepting friend requests
- Add friend_requests table in DatabaseSchemaService
- Add indexes for performance
- Register service in Program.cs DI container"

# Push to remote
git push -u origin claude/claude-md-mhza4i3nh0ixg07a-012Uos5coK1Tt9eHxJD3EbgA
```

### Git Safety Rules

- ✅ Always push to branch starting with `claude/` and ending with session ID
- ❌ Never force push to main/master
- ❌ Never skip hooks (--no-verify) unless explicitly requested
- ✅ Use descriptive commit messages (explain WHY, not just WHAT)
- ✅ Commit related changes together

---

## Additional Resources

### Documentation Files

- **README.md** - Installation guide (French)
- **PROJECT_STATUS.md** - Current status & checklist
- **CHANGELOG.md** - Version history
- **subject.md** - School project requirements

### Key Files to Reference

When working on specific areas, reference these files:

| Task | Reference Files |
|------|----------------|
| **Authentication** | `Services/CompleteAuthService.cs` (572 lines) |
| **User Management** | `Services/UserService.cs` (526 lines) |
| **Matching Logic** | `Services/MatchingService.cs` (444 lines) |
| **Chat** | `Services/MessageService.cs`, `Hubs/ChatHub.cs` |
| **Database Schema** | `Services/DatabaseSchemaService.cs` (225 lines) |
| **Security** | `Services/InputSanitizer.cs`, `Services/PhotoService.cs` |
| **Data Seeding** | `Services/DataSeederService.cs` (305 lines) |

---

## Summary

This is a **security-hardened, production-ready dating application** built with:

- ✅ **Manual SQL with Dapper** (no ORM - project requirement)
- ✅ **Comprehensive security** (BCrypt, XSS, SQL injection, CSRF, upload validation)
- ✅ **Real-time features** (SignalR for chat and notifications)
- ✅ **60+ PostgreSQL indexes** for performance
- ✅ **500 auto-generated profiles** with interactions
- ✅ **Clean architecture** (service layer, DI, separation of concerns)

**For AI assistants:**

1. **Always use parameterized queries** - String interpolation = project failure
2. **Follow security patterns** - Sanitize, validate, hash, encrypt
3. **Reference existing services** - Maintain consistency with established patterns
4. **Test with demo user** - `demo` / `Demo123!`
5. **Check logs** - All services log to console
6. **Build before committing** - `dotnet build`

**Need help?** Check README.md for setup, or inspect similar service files for patterns.

---

**End of CLAUDE.md**
