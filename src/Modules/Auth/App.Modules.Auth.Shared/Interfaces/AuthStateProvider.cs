using App.Models.Auth.Shared.ActionsRequests;
using App.Modules.Auth.Core.Entities;

namespace App.Models.Auth.Shared.Interfaces;

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

public sealed class AuthStateProvider(IAuthService api, NavigationManager nav, ILogger<AuthStateProvider> logger)
    : AuthenticationStateProvider
{
    private readonly ILoggingIn _loginApi = api;
    private readonly IRegistration _registerApi = api;
    private readonly IUserToken _tokenApi = api;
    private readonly ICurrentUserInfo _userInfoApi = api;

    private CurrentUser? _currentUser;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        try
        {
            var userInfo = await CurrentUserInfo();
            if (userInfo!.IsAuthenticated)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, _currentUser!.UserName!) }.Concat(_currentUser.Claims!.Select(c => new Claim(c.Key, c.Value))).ToList();
                var roles = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                claims.Remove(roles!);
                var rolesString = JsonSerializer.Deserialize<string[]>(roles!.Value);

                if (rolesString != null)
                    foreach (var role in rolesString!)
                        claims.Add(new Claim(ClaimTypes.Role, role));

                identity = new ClaimsIdentity(claims, "Server authentication");
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Request failed");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<bool> IsUserAuthenticated()
    {
        if (_currentUser is not null)
            return _currentUser.IsAuthenticated;

        return (await CurrentUserInfo())!.IsAuthenticated;
    }

    private async Task<CurrentUser?> CurrentUserInfo()
    {
        if (_currentUser is not null && _currentUser.IsAuthenticated) 
            return _currentUser;

        _currentUser = await _userInfoApi.GetCurrentUserInfo();

        return _currentUser;
    }

    public async Task<(bool Success, string? Msg)> Login(LoginRequest? loginRequest)
    {
        var loginResult = await _loginApi.Login(loginRequest);
        
        if (loginResult.Success)
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        return loginResult;
    }

    public async Task<bool> TryLogin(string password)
    {
        try
        {
            var result = await _loginApi.TryLogin(new LoginRequest { Password = password, Email = _currentUser!.UserName });
            return result;
        }
        catch(Exception ex)
        {
            logger.LogError("Error thrown during test login: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    public async Task<(bool Success, string? Msg)> Register(RegisterRequest? registerRequest)
    {
        var registerResult = await _registerApi.Register(registerRequest);

        if (!registerResult.Validation.Success)
            return (false, registerResult.Validation.Msg);

        return (true, registerResult.UserName);
    }

    public async Task Logout()
    {
        try
        {
            _currentUser!.IsAuthenticated = false;
            await _loginApi.Logout();

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception)
        {
            nav.NavigateTo("/");
        }
    }

    /// <summary>
    /// Get current user token, if the token is empty or invalid then redirect to ConfirmPassword page
    /// </summary>
    /// <param name="password">Password typed by user</param>
    /// <param name="pageName">Page scheme (every '/' must be replaced with '-') like: module-page-subpage-[...]</param>
    /// <returns>Token string</returns>
    public async Task<string?> GetCurrentUserToken(string? password = "", string? pageName = "")
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            if (string.IsNullOrWhiteSpace(_tokenApi.UserToken?.Token) || _tokenApi.UserToken.ExpirationDate < DateTime.UtcNow)
                nav.NavigateTo($"/account/ConfirmPassword/{pageName}");

            return _tokenApi.UserToken!.Token;
        }

        try
        {
            var currentUser = await CurrentUserInfo();
            await _tokenApi.RefreshToken(new LoginRequest() { Password = password, Email = currentUser?.UserName });
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error during refreshing token");
        }

        return _tokenApi.UserToken?.Token;
    }
}