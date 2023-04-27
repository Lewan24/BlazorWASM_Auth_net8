using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WASMWithAuth.Client.Authentication.Account;
using WASMWithAuth.Client.Authentication.Interfaces;
using WASMWithAuth.Shared.Entities;
using WASMWithAuth.Shared.Entities.Models;

namespace WASMWithAuth.Client.Authentication.Services
{
    public class CustomStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService api;
        private CurrentUser _currentUser;
        private NavigationManager _nav;

        public CustomStateProvider(IAuthService api, NavigationManager nav)
        {
            this.api = api;
            _nav = nav;
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

        public async Task<string> Login(LoginRequest loginRequest)
        {
            var token = await api.Login(loginRequest);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return token;
        }

        public async Task<bool> TryLogin(string password)
        {
            return await api.TryLogin(new LoginRequest{Password = password, UserName = _currentUser.UserName});
        }

        public async Task Register(RegisterRequest registerRequest)
        {
            await api.Register(registerRequest);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<string> EncryptToken(string token, string key)
        {
            return await api.EncryptToken(token, key);
        }

        public async Task<string> DecryptToken(string token, string key)
        {
            return await api.DecryptToken(token, key);
        }

        public async Task Logout()
        {
            await api.Logout();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<UserToken> GetUserToken(string? password = "", string? pageName = "")
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                if (string.IsNullOrWhiteSpace(api.UserToken.Token) || api.UserToken.ExpirationDate < DateTime.UtcNow)
                    _nav.NavigateTo($"/account/ConfirmPassword/{pageName}");
                return api.UserToken;
            }
            
            await api.RefreshToken(new LoginRequest() { Password = password, UserName = _currentUser.UserName });

            return api.UserToken;
        }
    }
}
