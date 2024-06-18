using App.Models.Auth.Shared.Interfaces.Token;
using App.Modules.Auth.Infrastructure.Repositories;
using OneOf;
using OneOf.Types;

namespace App.Modules.Auth.Application.Services.Token;

internal class TokenValidationService(IUserTokenService tokensRepoService) : ITokenValidationService
{
    public OneOf<True, False> IsValid(string? token, string? userEmail)
    {
        var userToken = tokensRepoService.GetUserTokens(t =>
            t.Token == token &&
            t.Email!.Equals(userEmail, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

        if (userToken is null)
            return new False();

        if (string.IsNullOrWhiteSpace(userToken.Token))
            return new False();

        if (userToken.ExpirationDate.AddMinutes(-1) >= DateTime.UtcNow)
            return new True();

        return new False();
    }
}