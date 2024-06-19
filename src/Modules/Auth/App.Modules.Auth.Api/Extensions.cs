using App.Modules.Auth.Application;
using App.Modules.Auth.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace App.Models.Auth.Api;

public static class Extensions
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddInfrastructureLayer();
        services.AddApplicationLayer();
        
        return services;
    }
}