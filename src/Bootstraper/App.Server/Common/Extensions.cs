using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace App.Server.Common;

public static class Extensions
{
    private const string DefaultCorsPolicyName = "DefaultPolicy";

    public static IServiceCollection AddBasicServerServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddRazorPages();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddOutputCache(opt =>
        {
            opt.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(5);
        });

        services.AddCors(opt =>
        {
            opt.AddPolicy(name: DefaultCorsPolicyName, policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyOrigin()
                    .AllowAnyMethod();
            });
        });
        
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = false;
            options.Events.OnRedirectToLogin = context =>
            {
                context.RedirectUri = "/";
                return Task.CompletedTask;
            };
            options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        });
        
        return services;
    }

    public static WebApplication UseBasicServerServices(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseSwagger();
        app.UseSwaggerUI();
        
        app.UseOutputCache();
        
        app.UseRouting();

        app.UseCors(DefaultCorsPolicyName);
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/_health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,

            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        }).CacheOutput();

        app.MapControllers();
        app.MapFallbackToFile("index.html");
        
        return app;
    }
}