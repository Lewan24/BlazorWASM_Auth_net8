using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity;
using WASMWithAuth.Server.Data.Interfaces;
using WASMWithAuth.Server.Models;
using WASMWithAuth.Shared.Entities;

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

    public async Task<bool> CheckValidation(string? token, string? userName)
    {
        var userToken = _context.UserTokens.FirstOrDefault(t => t.Token == token && t.UserName == userName && t.IsInActive == false);

        if (userToken is null)
            return false;

        if (string.IsNullOrWhiteSpace(userToken.Token))
            return false;

        return await CheckExpiration(userToken);
    }

    public Task<bool> CheckExpiration(UserToken userToken)
    {
        if (userToken.ExpirationDate < DateTime.UtcNow)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }
}