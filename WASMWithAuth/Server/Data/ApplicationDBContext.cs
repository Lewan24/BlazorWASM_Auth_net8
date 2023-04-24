using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WASMWithAuth.Server.Models;

namespace WASMWithAuth.Server.Data
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options):base(options){}

        //public DbSet<Developer> Developers { get; set; }
    }
}
