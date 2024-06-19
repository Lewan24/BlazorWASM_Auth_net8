using App.Modules.Auth.Infrastructure.DbContexts;
using App.Modules.Auth.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Modules.Auth.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        var connectionString = config["ConnectionStrings:DefaultConnection"]!;

        services.AddDbContext<AppIdentityDbContext>(opt =>
        {
            opt.UseMySQL(connectionString);
        });

        services.AddTransient<IUserTokenService, UserTokenService>();
        
        return services;
    }
}