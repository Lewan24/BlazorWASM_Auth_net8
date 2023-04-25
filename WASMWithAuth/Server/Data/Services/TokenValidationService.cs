using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity;
using WASMWithAuth.Server.Data.Interfaces;
using WASMWithAuth.Server.Models;

namespace WASMWithAuth.Server.Data.Services;

public class TokenValidationService : ITokenValidationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDBContext _context;

    public TokenValidationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDBContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public Task<bool> CheckValidation(string? token, string? userName)
    {
        var userToken = _context.UserTokens.FirstOrDefault(t => t.Token == token && t.UserName == userName && t.IsInActive == false);

        if (string.IsNullOrWhiteSpace(userToken.Token))
            return Task.FromResult(false);

        if (!CheckExpiration(token).Result)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    public Task<bool> CheckExpiration(string token)
    {
        var userToken = _context.UserTokens.FirstOrDefault(t => t.Token == token);

        if (userToken.ExpirationDate < DateTime.Now)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }
}