using System.Net;
using App.Models.Auth.Shared.Interfaces.Health;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;

namespace App.Modules.Auth.Application.Services.Health;

internal sealed class AuthHealthService(HttpClient httpClient, ILogger<AuthHealthService> logger) : IAuthHealthService
{
    public async Task<OneOf<True, False,  Error>> CheckHealthAsync()
    {
        try
        {
            var result = await httpClient.GetAsync("_health");
            if (result.StatusCode is HttpStatusCode.OK)
                return new True();

            return new False();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error has been thrown during checking _health state for Auth");
            
            return new Error();
        }
    }
}