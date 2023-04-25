using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WASMWithAuth.Server.Models;
using WASMWithAuth.Shared.Entities;

namespace WASMWithAuth.Server.Data
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options):base(options){}

        //public DbSet<Developer> Developers { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
    }
}
