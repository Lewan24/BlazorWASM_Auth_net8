using WASMWithAuth.Server.Data.Interfaces;

namespace WASMWithAuth.Server.Data.Services;

public class TokenValidationService : ITokenValidationService
{
    private readonly ApplicationDBContext _context;

    public TokenValidationService(ApplicationDBContext context)
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