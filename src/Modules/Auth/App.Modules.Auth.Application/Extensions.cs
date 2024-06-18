using App.Models.Auth.Shared.Interfaces;
using App.Models.Auth.Shared.Interfaces.Health;
using App.Models.Auth.Shared.Interfaces.Token;
using App.Modules.Auth.Application.Services;
using App.Modules.Auth.Application.Services.Health;
using App.Modules.Auth.Application.Services.Token;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace App.Modules.Auth.Application;

public static class Extensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddTransient<ITokenValidationService, TokenValidationService>();

        return services;
    }

    public static IServiceCollection AddClientApplicationLayer(this IServiceCollection services)
    {
        services.AddOptions();
        services.AddAuthorizationCore();
        
        services.AddSingleton<IAuthService, AuthService>();
        services.AddScoped<AuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<AuthStateProvider>());

        services.AddTransient<IAuthHealthService, AuthHealthService>();

        return services;
    }
}