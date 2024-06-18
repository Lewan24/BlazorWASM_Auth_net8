using App.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace App.Server.Data;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public new DbSet<UserToken> UserTokens { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.Migrate();
    }
}