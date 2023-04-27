using WASMWithAuth.Shared.Entities;
using WASMWithAuth.Shared.Entities.Models;

namespace WASMWithAuth.Client.Authentication.Interfaces;
public interface IAuthService
{
    UserToken? UserToken { get; set; }

    Task<string> Login(LoginRequest loginRequest);
    Task<bool> TryLogin(LoginRequest request);
    Task Register(RegisterRequest registerRequest);
    Task<string> EncryptToken(string token, string key);
    Task<string> DecryptToken(string encryptedtoken, string key);
    Task Logout();
    Task<CurrentUser> GetCurrentUserInfo();
    Task RefreshToken(LoginRequest request);
}
