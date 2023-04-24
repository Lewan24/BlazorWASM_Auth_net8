using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using WASMWithAuth.Client.Authentication.Interfaces;
using WASMWithAuth.Shared.Entities.Models;

namespace WASMWithAuth.Client.Authentication.Services
{
    public class CustomStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService api;
        private CurrentUser _currentUser;

        public CustomStateProvider(IAuthService api)
        {
            this.api = api;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userInfo = await CurrentUserInfo();
                if (userInfo.IsAuthenticated)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, _currentUser.UserName) }.Concat(_currentUser.Claims.Select(c => new Claim(c.Key, c.Value))).ToList();
                    var roles = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                    claims.Remove(roles);
                    var rolesString = JsonSerializer.Deserialize<string[]>(roles.Value);
                    foreach (var role in rolesString)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    identity = new ClaimsIdentity(claims, "Server authentication");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex.ToString());
            }
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private async Task<CurrentUser> CurrentUserInfo()
        {
            if (_currentUser is not null && _currentUser.IsAuthenticated) return _currentUser;
            _currentUser = await api.GetCurrentUserInfo();
            return _currentUser;
        }

        public async Task Login(LoginRequest loginRequest)
        {
            await api.Login(loginRequest);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Register(RegisterRequest registerRequest)
        {
            await api.Register(registerRequest);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            await api.Logout();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
