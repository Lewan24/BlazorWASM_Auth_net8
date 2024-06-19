using App.Models.Auth.Shared.ActionsRequests;
using App.Models.Auth.Shared.Entities.Tokens;
using App.Models.Auth.Shared.Interfaces;
using App.Modules.Auth.Core.Entities;
using System.Net;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace App.Modules.Auth.Application.Services;

internal class AuthService(HttpClient httpClient) : IAuthService
{
    public TokenModelDto? UserToken { get; set; } = new();

    public async Task<(bool Success, string? Msg)> Login(LoginRequest? loginRequest)
    {
        var loginResult = await httpClient.PostAsJsonAsync("api/Auth/Login", loginRequest);
        
        if (loginResult.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            return (false, await loginResult.Content.ReadAsStringAsync());

        var retrieveUserTokenResult = await httpClient.PostAsJsonAsync("api/Auth/GetUserToken", loginRequest);
        UserToken = JsonConvert.DeserializeObject<TokenModelDto>(await retrieveUserTokenResult.Content.ReadAsStringAsync());

        return (true, null);
    }

    public async Task<bool> TryLogin(LoginRequest? request)
    {
        var result = await httpClient.PostAsJsonAsync("api/Auth/CanLogIn", request);
        result.EnsureSuccessStatusCode();

        return JsonConvert.DeserializeObject<bool>(await result.Content.ReadAsStringAsync());
    }

    public async Task<((bool Success, string? Msg) Validation, string? UserName)> Register(RegisterRequest? registerRequest)
    {
        var result = await httpClient.PostAsJsonAsync("api/Auth/Register", registerRequest);
        if (result.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            return new(new(false, await result.Content.ReadAsStringAsync()), null);

        return new(new(true, null), await result.Content.ReadAsStringAsync());
    }

    public async Task Logout() 
        => await httpClient.PostAsync("api/Auth/Logout", null);

    public async Task<CurrentUser?> GetCurrentUserInfo()
        => await httpClient.GetFromJsonAsync<CurrentUser>("api/Auth/GetCurrentUser");

    public async Task RefreshToken(LoginRequest request)
    {
        var result = await httpClient.PostAsJsonAsync("api/Auth/RefreshToken", request);

        UserToken = JsonConvert.DeserializeObject<TokenModelDto>(await result.Content.ReadAsStringAsync());
    }
}