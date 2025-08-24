using Microsoft.AspNetCore.Http;
using WebMatcha.Models;

namespace WebMatcha.Services;

public class ServerSessionService : IServerSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserService _userService;
    
    public ServerSessionService(IHttpContextAccessor httpContextAccessor, UserService userService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
    }
    
    public int? GetCurrentUserId()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session?.GetInt32("CurrentUserId") is int userId)
        {
            return userId;
        }
        return null;
    }
    
    public string? GetCurrentUsername()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return session?.GetString("CurrentUsername");
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
    
    public void SetCurrentUser(User user)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            session.SetInt32("CurrentUserId", user.Id);
            session.SetString("CurrentUsername", user.Username);
        }
    }
    
    public void ClearSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Clear();
    }
}