using WASMWithAuth.Shared.Entities.Models;

namespace WASMWithAuth.Client.Authentication.Interfaces;
public interface IAuthService
{
    Task Login(LoginRequest loginRequest);
    Task Register(RegisterRequest registerRequest);
    Task Logout();
    Task<CurrentUser> GetCurrentUserInfo();
}
