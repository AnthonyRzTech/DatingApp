using WebMatcha.Models;

namespace WebMatcha.Services;

public interface IServerSessionService
{
    void SetCurrentUser(User user);
    int? GetCurrentUserId();
    string? GetCurrentUsername();
    bool IsAuthenticated();
    void ClearSession();
}