using System.Net;
using System.Net.Http.Json;
using WASMWithAuth.Client.Authentication.Interfaces;
using WASMWithAuth.Shared.Entities.Models;

namespace WASMWithAuth.Client.Authentication.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Login(LoginRequest loginRequest)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Auth/Login", loginRequest);
        if (result.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();
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
}