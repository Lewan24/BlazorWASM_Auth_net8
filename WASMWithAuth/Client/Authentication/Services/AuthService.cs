using System.Net;
using System.Net.Http.Json;
using Newtonsoft.Json;
using WASMWithAuth.Client.Authentication.Account;
using WASMWithAuth.Client.Authentication.Interfaces;
using WASMWithAuth.Shared.Entities;
using WASMWithAuth.Shared.Entities.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WASMWithAuth.Client.Authentication.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public UserToken? UserToken { get; set; } = new();

    public async Task Login(LoginRequest loginRequest)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Auth/Login", loginRequest);
        if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
            throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();

        var result2 = await _httpClient.PostAsJsonAsync("api/Auth/GetUserToken", loginRequest);
        result2.EnsureSuccessStatusCode();

        var jsonstring = await result2.Content.ReadAsStringAsync();

        UserToken = JsonConvert.DeserializeObject<UserToken>(jsonstring);
    }

    public async Task<bool> TryLogin(LoginRequest request)
    {
        var result =  await _httpClient.PostAsJsonAsync<LoginRequest>("api/Auth/TryLogin", request);
        result.EnsureSuccessStatusCode();

        return bool.Parse(result.Content.ReadAsStringAsync().Result);
    }

    public async Task Register(RegisterRequest registerRequest)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Auth/Register", registerRequest);
        if (result.StatusCode == HttpStatusCode.BadRequest)
            throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();
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

    public async Task RefreshToken(string username)
    {
        //TODO: Returns basically badrequest. Cant refresh token while on main page token is null
        var result = await _httpClient.PostAsJsonAsync("api/Auth/RefreshToken", username);
        result.EnsureSuccessStatusCode();

        UserToken = JsonConvert.DeserializeObject<UserToken>(await result.Content.ReadAsStringAsync());
    }
}