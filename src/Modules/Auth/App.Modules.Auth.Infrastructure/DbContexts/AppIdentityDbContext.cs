using App.Models.Auth.Shared.Static.Entities;
using App.Modules.Auth.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Auth.Infrastructure.DbContexts;

public sealed class AppIdentityDbContext : IdentityDbContext<AppUser>
{
    public DbSet<TokenModel> UsersTokens { get; set; }

    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
    {
        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData([
            new IdentityRole() { Id = "0A10BC5B-7C17-47E8-943D-202A07D47372", Name = AppRoles.User,  NormalizedName = AppRoles.User.ToUpper() },
            new IdentityRole() { Id = "D731745F-79A0-4B84-B785-2558F75238C0", Name = AppRoles.Admin, NormalizedName = AppRoles.Admin.ToUpper() }
        ]);

        base.OnModelCreating(builder);
    }
}