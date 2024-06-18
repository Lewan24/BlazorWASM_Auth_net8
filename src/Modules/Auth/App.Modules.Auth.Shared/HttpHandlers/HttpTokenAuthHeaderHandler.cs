using App.Models.Auth.Shared.Interfaces;
using App.Models.Auth.Shared.Static.Setters;

namespace App.Models.Auth.Shared.HttpHandlers;

public class HttpTokenAuthHeaderHandler(IAuthService authApi) : DelegatingHandler
{
    private readonly IUserToken _userTokenApi = authApi;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Current token: {_userTokenApi.UserToken!.Token}");
        HttpClientAuthHeaderSetter.SetToken(ref request, _userTokenApi.UserToken!.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}