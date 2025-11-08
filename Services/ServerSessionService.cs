using Microsoft.AspNetCore.Http;
using WebMatcha.Models;

namespace WebMatcha.Services;

public class ServerSessionService : IServerSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserService _userService;
    private readonly ILogger<ServerSessionService> _logger;

    private const string SESSION_USER_ID = "CurrentUserId";
    private const string SESSION_USERNAME = "CurrentUsername";
    private const string SESSION_CREATED_AT = "SessionCreatedAt";
    private const string SESSION_IP = "SessionIP";
    private const string SESSION_USER_AGENT = "SessionUserAgent";
    private const int SESSION_TIMEOUT_MINUTES = 30;

    public ServerSessionService(
        IHttpContextAccessor httpContextAccessor,
        UserService userService,
        ILogger<ServerSessionService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
        _logger = logger;
    }
    
    public int? GetCurrentUserId()
    {
        if (!ValidateSession())
        {
            ClearSession();
            return null;
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        if (session?.GetInt32(SESSION_USER_ID) is int userId)
        {
            return userId;
        }
        return null;
    }

    public string? GetCurrentUsername()
    {
        if (!ValidateSession())
        {
            ClearSession();
            return null;
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        return session?.GetString(SESSION_USERNAME);
    }
    
    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            return await _userService.GetUserByIdAsync(userId.Value);
        }
        return null;
    }

    public bool IsAuthenticated()
    {
        var userId = GetCurrentUserId();
        return userId.HasValue;
    }

    // Async wrapper methods for compatibility with Blazor components
    public Task<int?> GetCurrentUserIdAsync()
    {
        return Task.FromResult(GetCurrentUserId());
    }

    public Task<string?> GetCurrentUsernameAsync()
    {
        return Task.FromResult(GetCurrentUsername());
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(IsAuthenticated());
    }

    public Task SetCurrentUserAsync(User user)
    {
        SetCurrentUser(user);
        return Task.CompletedTask;
    }

    public Task ClearSessionAsync()
    {
        ClearSession();
        return Task.CompletedTask;
    }
    
    public void SetCurrentUser(User user)
    {
        var context = _httpContextAccessor.HttpContext;
        var session = context?.Session;
        if (session != null && context != null)
        {
            // Clear old session data for security
            session.Clear();

            // Set user data
            session.SetInt32(SESSION_USER_ID, user.Id);
            session.SetString(SESSION_USERNAME, user.Username);

            // Set security metadata
            session.SetString(SESSION_CREATED_AT, DateTime.UtcNow.ToString("o"));
            session.SetString(SESSION_IP, GetClientIP(context));
            session.SetString(SESSION_USER_AGENT, context.Request.Headers["User-Agent"].ToString());

            _logger.LogInformation("Session created for user {UserId} from IP {IP}", user.Id, GetClientIP(context));
        }
    }

    public void ClearSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            var userId = session.GetInt32(SESSION_USER_ID);
            session.Clear();
            _logger.LogInformation("Session cleared for user {UserId}", userId);
        }
    }

    private bool ValidateSession()
    {
        var context = _httpContextAccessor.HttpContext;
        var session = context?.Session;

        if (session == null || context == null)
            return false;

        // Check if user ID exists in session
        if (!session.GetInt32(SESSION_USER_ID).HasValue)
            return false;

        // Validate session timeout
        var createdAtStr = session.GetString(SESSION_CREATED_AT);
        if (!string.IsNullOrEmpty(createdAtStr))
        {
            if (DateTime.TryParse(createdAtStr, out var createdAt))
            {
                if (DateTime.UtcNow - createdAt > TimeSpan.FromMinutes(SESSION_TIMEOUT_MINUTES))
                {
                    _logger.LogWarning("Session expired for user {UserId}", session.GetInt32(SESSION_USER_ID));
                    return false;
                }
            }
        }

        // Validate IP address (prevent session hijacking)
        var sessionIP = session.GetString(SESSION_IP);
        var currentIP = GetClientIP(context);
        if (!string.IsNullOrEmpty(sessionIP) && sessionIP != currentIP)
        {
            _logger.LogWarning("Session IP mismatch for user {UserId}. Expected {SessionIP}, got {CurrentIP}",
                session.GetInt32(SESSION_USER_ID), sessionIP, currentIP);
            return false;
        }

        // Validate User-Agent (additional security layer)
        var sessionUA = session.GetString(SESSION_USER_AGENT);
        var currentUA = context.Request.Headers["User-Agent"].ToString();
        if (!string.IsNullOrEmpty(sessionUA) && sessionUA != currentUA)
        {
            _logger.LogWarning("Session User-Agent mismatch for user {UserId}",
                session.GetInt32(SESSION_USER_ID));
            return false;
        }

        return true;
    }

    private string GetClientIP(HttpContext context)
    {
        // Try to get real IP from headers (in case of proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIP))
        {
            return realIP;
        }

        // Fallback to direct connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}