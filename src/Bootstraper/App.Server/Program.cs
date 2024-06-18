using App.Models.Auth.Api;
using App.Modules.Auth.Core.Entities;
using App.Modules.Auth.Infrastructure.DbContexts;
using App.Server.Common;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBasicServerServices();
builder.Services.AddAuthModule()
    .AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

var config = builder.Configuration;
var connectionString = config.GetConnectionString("DefaultConnection");
    
builder.Services.AddHealthChecks()
    .AddMySql(connectionString!, name: "AppDb");

var app = builder.Build();

app.UseBasicServerServices();

app.Run();
