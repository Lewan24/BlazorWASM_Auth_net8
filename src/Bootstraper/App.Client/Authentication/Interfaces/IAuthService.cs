using Shared.Entities;
using Shared.Entities.Models;

namespace App.Client.Authentication.Interfaces;

public interface IAuthService
{
    UserToken? UserToken { get; set; }

    Task<string> Login(LoginRequest loginRequest);
    Task<bool> TryLogin(LoginRequest request);
    Task Register(RegisterRequest registerRequest);
    Task<string> EncryptToken(TokenKeyModel request);
    Task<string> DecryptToken(TokenKeyModel request);
    Task Logout();
    Task<CurrentUser> GetCurrentUserInfo();
    Task RefreshToken(LoginRequest request);
}
