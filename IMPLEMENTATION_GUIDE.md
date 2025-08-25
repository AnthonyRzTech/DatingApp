# WebMatcha Implementation Guide

This guide details the remaining implementation tasks to complete the WebMatcha project according to the 42 School requirements.

## 🎯 Project Requirements Checklist

### ✅ Completed Features
- [x] User registration with validation
- [x] Password hashing with BCrypt
- [x] Login/logout functionality
- [x] Email service integration
- [x] Profile editing with photo upload
- [x] Geolocation features
- [x] Browse profiles with filters
- [x] Advanced search functionality
- [x] Real-time chat with SignalR
- [x] Like/unlike functionality
- [x] Matching system
- [x] Profile view tracking
- [x] Fame rating calculation
- [x] Block and report users
- [x] Interest tags
- [x] Notification system
- [x] Protected routes

### ⚠️ Required Implementations

## 1. Email Verification System 🔐

### Current State
- CompleteAuthService has email verification logic
- EmailService configured with SMTP
- Verification pages created

### Required Steps
```csharp
// 1. Update registration to require email verification
// In Register.razor
private async Task HandleRegister()
{
    var result = await AuthService.RegisterAsync(registerRequest);
    if (result.Success)
    {
        // Show message: "Check your email to verify your account"
        showVerificationMessage = true;
    }
}

// 2. Create email verification page
@page "/verify-email/{token}"
// Auto-verify and redirect to login

// 3. Block login until email verified
// In CompleteAuthService.LoginAsync
if (!user.EmailVerified)
{
    return new AuthResult 
    { 
        Success = false, 
        Errors = new[] { "Please verify your email first" } 
    };
}
```

## 2. JWT Authentication 🎫

### Replace Session with JWT
```csharp
// 1. Configure JWT in Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// 2. Create JWT service
public class JwtService
{
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);
            
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// 3. Update login to return JWT
var token = _jwtService.GenerateToken(user);
return new { token, user };
```

## 3. Manual SQL Queries 📊

### Convert LINQ to Raw SQL
```csharp
// Example: GetSuggestedUsers with manual query
public async Task<List<User>> GetSuggestedUsersAsync(int userId, UserPreferences prefs)
{
    var sql = @"
        SELECT u.*, 
               ST_Distance(
                   ST_MakePoint(u.longitude, u.latitude)::geography,
                   ST_MakePoint(@UserLong, @UserLat)::geography
               ) / 1000 as distance,
               COUNT(DISTINCT ut.tag_id) as common_tags
        FROM users u
        LEFT JOIN user_tags ut ON ut.user_id = u.id
        LEFT JOIN user_tags mut ON mut.tag_id = ut.tag_id AND mut.user_id = @UserId
        WHERE u.id != @UserId
          AND u.age BETWEEN @MinAge AND @MaxAge
          AND u.sexual_orientation IN (@Orientations)
          AND NOT EXISTS (
              SELECT 1 FROM blocks b 
              WHERE (b.blocker_id = @UserId AND b.blocked_id = u.id)
                 OR (b.blocker_id = u.id AND b.blocked_id = @UserId)
          )
        GROUP BY u.id
        HAVING ST_Distance(
            ST_MakePoint(u.longitude, u.latitude)::geography,
            ST_MakePoint(@UserLong, @UserLat)::geography
        ) / 1000 <= @MaxDistance
        ORDER BY 
            common_tags DESC,
            distance ASC,
            u.fame_rating DESC
        LIMIT 50";
    
    return await _context.Users
        .FromSqlRaw(sql, parameters)
        .ToListAsync();
}

// Example: Update fame rating
public async Task UpdateFameRatingAsync(int userId)
{
    var sql = @"
        UPDATE users 
        SET fame_rating = (
            SELECT 
                COALESCE(like_score, 0) * 0.3 +
                COALESCE(view_score, 0) * 0.2 +
                COALESCE(match_score, 0) * 0.3 +
                COALESCE(profile_score, 0) * 0.2
            FROM (
                SELECT 
                    u.id,
                    (SELECT COUNT(*) * 5 FROM likes WHERE liked_id = u.id) as like_score,
                    (SELECT COUNT(*) FROM profile_views WHERE viewed_id = u.id) as view_score,
                    (SELECT COUNT(*) * 10 FROM matches WHERE user1_id = u.id OR user2_id = u.id) as match_score,
                    CASE 
                        WHEN u.bio IS NOT NULL AND LENGTH(u.bio) > 50 THEN 20
                        ELSE 0
                    END +
                    CASE 
                        WHEN u.profile_photo_url IS NOT NULL THEN 20
                        ELSE 0
                    END +
                    (SELECT COUNT(*) * 10 FROM user_tags WHERE user_id = u.id) as profile_score
                FROM users u
                WHERE u.id = @UserId
            ) scores
        )
        WHERE id = @UserId";
    
    await _context.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@UserId", userId));
}
```

## 4. FastEndpoints Integration 🚀

### Convert to Vertical Slice Architecture
```csharp
// Features/Auth/Login/LoginEndpoint.cs
public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly CompleteAuthService _authService;
    private readonly JwtService _jwtService;
    
    public LoginEndpoint(CompleteAuthService authService, JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }
    
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Validator<LoginValidator>();
    }
    
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(req);
        
        if (!result.Success)
        {
            await SendErrorsAsync(result.Errors.ToList());
            return;
        }
        
        var token = _jwtService.GenerateToken(result.User);
        
        await SendOkAsync(new LoginResponse
        {
            Token = token,
            User = result.User.ToDto()
        });
    }
}

// Features/Auth/Login/LoginValidator.cs
public class LoginValidator : Validator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
```

## 5. Real-time Notifications Hub 📢

### SignalR NotificationHub
```csharp
// Hubs/NotificationHub.cs
public class NotificationHub : Hub
{
    private static readonly Dictionary<int, string> _userConnections = new();
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.GetUserId();
        if (userId.HasValue)
        {
            _userConnections[userId.Value] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }
    
    public async Task SendNotificationToUser(int userId, Notification notification)
    {
        await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", notification);
    }
    
    public async Task MarkAsRead(int notificationId)
    {
        // Update database
        await _notificationService.MarkAsReadAsync(notificationId);
    }
}

// Update services to use hub
public class MatchingService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    
    public async Task CreateMatchAsync(int user1Id, int user2Id)
    {
        // Create match in database
        
        // Send real-time notifications
        await _hubContext.Clients.Group($"user-{user1Id}")
            .SendAsync("NewMatch", user2);
        await _hubContext.Clients.Group($"user-{user2Id}")
            .SendAsync("NewMatch", user1);
    }
}
```

## 6. Database Optimizations 🔧

### Add Indexes
```sql
-- Performance indexes
CREATE INDEX idx_users_location ON users USING GIST (ST_MakePoint(longitude, latitude));
CREATE INDEX idx_users_age_fame ON users (age, fame_rating);
CREATE INDEX idx_likes_user_liked ON likes (user_id, liked_id);
CREATE INDEX idx_messages_conversation ON messages (sender_id, receiver_id, sent_at DESC);
CREATE INDEX idx_notifications_user_unread ON notifications (user_id, is_read) WHERE is_read = false;
CREATE INDEX idx_profile_views_viewed ON profile_views (viewed_id, viewed_at DESC);

-- Composite indexes for common queries
CREATE INDEX idx_user_search ON users (sexual_orientation, age, fame_rating);
CREATE INDEX idx_user_tags_lookup ON user_tags (user_id, tag_id);
```

### Add Constraints
```sql
-- Ensure data integrity
ALTER TABLE users ADD CONSTRAINT chk_age CHECK (age >= 18);
ALTER TABLE users ADD CONSTRAINT chk_fame CHECK (fame_rating >= 0 AND fame_rating <= 100);
ALTER TABLE likes ADD CONSTRAINT uk_likes UNIQUE (user_id, liked_id);
ALTER TABLE blocks ADD CONSTRAINT uk_blocks UNIQUE (blocker_id, blocked_id);
ALTER TABLE matches ADD CONSTRAINT uk_matches UNIQUE (user1_id, user2_id);
```

## 7. Security Hardening 🛡️

### Additional Security Measures
```csharp
// 1. Rate limiting for all endpoints
app.UseRateLimiter(new RateLimiterOptions
{
    GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }))
});

// 2. Content Security Policy
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' wss:;");
    await next();
});

// 3. Request validation middleware
public class RequestValidationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Validate file uploads
        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();
            foreach (var file in form.Files)
            {
                if (file.Length > 5 * 1024 * 1024) // 5MB
                {
                    context.Response.StatusCode = 413;
                    await context.Response.WriteAsync("File too large");
                    return;
                }
            }
        }
        await _next(context);
    }
}
```

## 8. Testing Implementation 🧪

### Unit Tests
```csharp
// Test matching algorithm
[Fact]
public async Task GetSuggestedUsers_ReturnsCompatibleUsers()
{
    // Arrange
    var user = new User { Id = 1, SexualOrientation = "heterosexual", Gender = "male" };
    var compatibleUser = new User { Id = 2, SexualOrientation = "heterosexual", Gender = "female" };
    var incompatibleUser = new User { Id = 3, SexualOrientation = "homosexual", Gender = "female" };
    
    // Act
    var suggestions = await _matchingService.GetSuggestedUsersAsync(user.Id);
    
    // Assert
    Assert.Contains(suggestions, u => u.Id == compatibleUser.Id);
    Assert.DoesNotContain(suggestions, u => u.Id == incompatibleUser.Id);
}

// Test fame rating calculation
[Theory]
[InlineData(10, 5, 2, 85)] // likes, views, matches, expected fame
[InlineData(0, 0, 0, 20)]   // base score for complete profile
public async Task CalculateFameRating_ReturnsCorrectScore(
    int likes, int views, int matches, int expectedFame)
{
    // Test implementation
}
```

### Integration Tests
```csharp
// Test complete user flow
[Fact]
public async Task UserFlow_RegisterLoginBrowseMatchChat_Works()
{
    // 1. Register user
    // 2. Verify email
    // 3. Login
    // 4. Complete profile
    // 5. Browse users
    // 6. Like user
    // 7. Create match
    // 8. Send message
    // 9. Verify notifications
}
```

## 9. Performance Optimizations ⚡

### Caching Strategy
```csharp
// Add memory cache for frequently accessed data
services.AddMemoryCache();

// Cache user preferences
public async Task<UserPreferences> GetUserPreferencesAsync(int userId)
{
    var cacheKey = $"user_prefs_{userId}";
    
    if (!_cache.TryGetValue(cacheKey, out UserPreferences prefs))
    {
        prefs = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
            
        _cache.Set(cacheKey, prefs, TimeSpan.FromMinutes(5));
    }
    
    return prefs;
}

// Cache suggested users
public async Task<List<User>> GetCachedSuggestionsAsync(int userId)
{
    var cacheKey = $"suggestions_{userId}";
    
    if (!_cache.TryGetValue(cacheKey, out List<User> suggestions))
    {
        suggestions = await GetSuggestedUsersAsync(userId);
        _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(10));
    }
    
    return suggestions;
}
```

## 10. Mobile Responsiveness 📱

### PWA Configuration
```json
// wwwroot/manifest.json
{
  "name": "WebMatcha Dating",
  "short_name": "WebMatcha",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#dc3545",
  "icons": [
    {
      "src": "/icons/icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/icons/icon-512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ]
}
```

### Service Worker
```javascript
// wwwroot/service-worker.js
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open('v1').then(cache => {
            return cache.addAll([
                '/',
                '/css/site.css',
                '/js/site.js',
                '/manifest.json'
            ]);
        })
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});
```

## Deployment Checklist 📋

### Pre-deployment
- [ ] All tests passing
- [ ] No console errors
- [ ] HTTPS configured
- [ ] Environment variables set
- [ ] Database backed up
- [ ] Logs configured
- [ ] Monitoring setup

### Post-deployment
- [ ] Verify email sending
- [ ] Test real-time features
- [ ] Check mobile responsiveness
- [ ] Monitor performance
- [ ] Review security headers
- [ ] Test with 500+ users

## Resources 📚

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr)
- [JWT Authentication](https://jwt.io/introduction)
- [OWASP Security Guidelines](https://owasp.org)

---

**Note**: This guide provides implementation details for completing the WebMatcha project. Follow the 42 School evaluation criteria and ensure all security requirements are met.