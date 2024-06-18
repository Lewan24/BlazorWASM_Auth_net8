using System.Net;
using System.Net.Http.Json;
using App.Client.Authentication.Interfaces;
using Newtonsoft.Json;
using Shared.Entities;
using Shared.Entities.Models;

namespace App.Client.Authentication.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public UserToken? UserToken { get; set; } = new();

    public async Task<string> Login(LoginRequest loginRequest)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Auth/Login", loginRequest);
        if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
            throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();

        var result2 = await _httpClient.PostAsJsonAsync("api/Auth/GetUserToken", loginRequest);
        result2.EnsureSuccessStatusCode();
        
        UserToken = JsonConvert.DeserializeObject<UserToken>(await result2.Content.ReadAsStringAsync());

        return UserToken.Token;
    }

    public async Task<bool> TryLogin(LoginRequest request)
    {
        var result =  await _httpClient.PostAsJsonAsync("api/Auth/TryLogin", request);
        result.EnsureSuccessStatusCode();

        return JsonConvert.DeserializeObject<bool>(await result.Content.ReadAsStringAsync());
    }

    public async Task Register(RegisterRequest registerRequest)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Auth/Register", registerRequest);
        if (result.StatusCode == HttpStatusCode.BadRequest)
            throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();
    }

    public async Task<string> EncryptToken(TokenKeyModel request)
    {
        var result = await _httpClient.PostAsJsonAsync($"api/Auth/GetEncryption", request);
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadAsStringAsync();
    }

    public async Task<string> DecryptToken(TokenKeyModel request)
    {
        var result = await _httpClient.PostAsJsonAsync($"api/Auth/GetDecryption", request);
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadAsStringAsync();
    }

    public async Task Logout()
    {
        var result = await _httpClient.PostAsync("api/Auth/Logout", null);
        result.EnsureSuccessStatusCode();
    }

    public async Task<CurrentUser> GetCurrentUserInfo()
    {
        var result = await _httpClient.GetFromJsonAsync<CurrentUser>("api/Auth/GetCurrentUser");
        return result;
    }

    public async Task RefreshToken(LoginRequest request)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Auth/RefreshToken", request);
        result.EnsureSuccessStatusCode();

        UserToken = JsonConvert.DeserializeObject<UserToken>(await result.Content.ReadAsStringAsync());
    }
}