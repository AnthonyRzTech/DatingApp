using Npgsql;
using Dapper;
using WebMatcha.Models;

namespace WebMatcha.Services;

/// <summary>
/// AuthService - Version simplifiée qui wraps CompleteAuthService
/// Note: CompleteAuthService est l'implémentation principale avec email verification
/// </summary>
public class AuthService : IAuthService
{
    private readonly CompleteAuthService _completeAuthService;

    public AuthService(CompleteAuthService completeAuthService)
    {
        _completeAuthService = completeAuthService;
    }

    public Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        return _completeAuthService.RegisterAsync(request);
    }

    public Task<AuthResult> LoginAsync(LoginRequest request)
    {
        return _completeAuthService.LoginAsync(request);
    }

    public Task LogoutAsync(int userId)
    {
        return _completeAuthService.LogoutAsync(userId);
    }

    public Task<bool> VerifyEmailAsync(string token)
    {
        return _completeAuthService.VerifyEmailAsync(token);
    }

    public Task<bool> SendPasswordResetAsync(string email)
    {
        return _completeAuthService.SendPasswordResetAsync(email);
    }

    public Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        return _completeAuthService.ResetPasswordAsync(token, newPassword);
    }

    public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        return _completeAuthService.ChangePasswordAsync(userId, currentPassword, newPassword);
    }

    public Task<bool> VerifyUserPasswordAsync(int userId, string password)
    {
        return _completeAuthService.VerifyUserPasswordAsync(userId, password);
    }
}
