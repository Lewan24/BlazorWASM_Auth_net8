using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WASMWithAuth.Client.Authentication.Interfaces;
using WASMWithAuth.Shared.Entities;
using WASMWithAuth.Shared.Entities.Models;

#pragma warning disable CS8603

namespace WASMWithAuth.Client.Authentication.Services;

public class CustomStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _api;
    private readonly NavigationManager _nav;

    private CurrentUser _currentUser;

    public CustomStateProvider(IAuthService api, NavigationManager nav)
    {
        _api = api;
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

                if (rolesString != null)
                    foreach (var role in rolesString!)
                        claims.Add(new Claim(ClaimTypes.Role, role));

                identity = new ClaimsIdentity(claims, "Server authentication");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Request failed:" + ex);
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private async Task<CurrentUser> CurrentUserInfo()
    {
        if (_currentUser is not null && _currentUser.IsAuthenticated) return _currentUser;
        _currentUser = await _api.GetCurrentUserInfo();
        return _currentUser;
    }

    public async Task<string> Login(LoginRequest loginRequest)
    {
        var token = await _api.Login(loginRequest);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return token;
    }

    public async Task<bool> TryLogin(string password) => 
        await _api.TryLogin(new LoginRequest{Password = password, UserName = _currentUser.UserName});

    public async Task Register(RegisterRequest registerRequest)
    {
        await _api.Register(registerRequest);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string> EncryptToken(TokenKeyModel request) => await _api.EncryptToken(request);
    public async Task<string> DecryptToken(TokenKeyModel request) => await _api.DecryptToken(request);

    public async Task Logout()
    {
        try
        {
            await _api.Logout();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception)
        {
            _nav.NavigateTo("/");
        }
    }

    public async Task<UserToken> GetUserToken(string? password = "", string? pageName = "")
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            if (string.IsNullOrWhiteSpace(_api.UserToken?.Token) || _api.UserToken.ExpirationDate < DateTime.UtcNow)
                _nav.NavigateTo($"/account/ConfirmPassword/{pageName}");
            
            return _api.UserToken;
        }
        
        await _api.RefreshToken(new LoginRequest() { Password = password, UserName = _currentUser.UserName });

        return _api.UserToken;
    }
}