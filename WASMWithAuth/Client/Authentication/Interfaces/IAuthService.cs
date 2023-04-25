using WASMWithAuth.Shared.Entities;
using WASMWithAuth.Shared.Entities.Models;

namespace WASMWithAuth.Client.Authentication.Interfaces;
public interface IAuthService
{
    UserToken? UserToken { get; set; }

    Task Login(LoginRequest loginRequest);
    Task<bool> TryLogin(LoginRequest request);
    Task Register(RegisterRequest registerRequest);
    Task Logout();
    Task<CurrentUser> GetCurrentUserInfo();
    Task RefreshToken(string username);
}
