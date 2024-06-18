using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using App.Server.Data;
using App.Server.Data.Interfaces;
using App.Server.Data.Services;
using App.Server.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseMySQL(connectionString!));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = false;
    options.Events.OnRedirectToLogin = context =>
    {
        context.RedirectUri = "/";
        return Task.CompletedTask;
    };
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
});

builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();

builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
