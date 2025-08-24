using Microsoft.JSInterop;
using WebMatcha.Models;

namespace WebMatcha.Services;

public class SessionService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly UserService _userService;
    
    public SessionService(IJSRuntime jsRuntime, UserService userService)
    {
        _jsRuntime = jsRuntime;
        _userService = userService;
    }
    
    public async Task<int?> GetCurrentUserIdAsync()
    {
        try
        {
            var userIdString = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "currentUserId");
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
        }
        catch (Exception)
        {
            // JSRuntime not available (server-side rendering)
        }
        return null;
    }
    
    public async Task<string?> GetCurrentUsernameAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "currentUsername");
        }
        catch (Exception)
        {
            // JSRuntime not available (server-side rendering)
            return null;
        }
    }
    
    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId.HasValue)
        {
            return await _userService.GetUserByIdAsync(userId.Value);
        }
        return null;
    }
    
    public async Task<bool> IsAuthenticatedAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        return userId.HasValue;
    }
    
    public async Task SetCurrentUserAsync(User user)
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "currentUserId", user.Id.ToString());
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "currentUsername", user.Username);
    }
    
    public async Task ClearSessionAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "currentUserId");
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "currentUsername");
    }
}