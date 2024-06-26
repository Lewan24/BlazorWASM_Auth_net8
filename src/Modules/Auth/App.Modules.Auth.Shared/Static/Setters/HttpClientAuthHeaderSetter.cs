using App.Models.Auth.Shared.Static.Entities;

namespace App.Models.Auth.Shared.Static.Setters;

internal class HttpClientAuthHeaderSetter
{
    /// <summary>
    /// Sets authorization header in http request message, the value is in-app token that is used in controllers to authorize and validate
    /// </summary>
    /// <param name="request"><see cref="HttpRequestMessage"/> that will be used to make a call</param>
    /// <param name="token">In app token that is generated while logging in user</param>
    public static void SetToken(ref HttpRequestMessage request, string? token)
    {
        if (request.Headers.Contains(AuthHeaderName.Name))
            request.Headers.Remove(AuthHeaderName.Name);

        request.Headers.Add(AuthHeaderName.Name, token);
    }
}