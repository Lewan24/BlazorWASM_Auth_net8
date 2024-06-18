using App.Server.Data.Interfaces;

namespace App.Server.Data.Services;

public class TokenValidationService : ITokenValidationService
{
    private readonly ApplicationDbContext _context;

    public TokenValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public bool CheckValidation(string? token, string? userName)
    {
        var userToken = _context.UserTokens.FirstOrDefault(t => 
            t.Token == token && 
            t.UserName == userName && 
            t.IsInActive == false);

        if (userToken is null)
            return false;

        if (string.IsNullOrWhiteSpace(userToken.Token))
            return false;

        return userToken.ExpirationDate.AddMinutes(-1) >= DateTime.UtcNow;
    }
}