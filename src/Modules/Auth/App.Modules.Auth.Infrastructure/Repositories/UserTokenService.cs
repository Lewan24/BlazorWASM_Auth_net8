using App.Models.Auth.Shared.Entities.Tokens;
using App.Modules.Auth.Infrastructure.DbContexts;

namespace App.Modules.Auth.Infrastructure.Repositories;

internal class UserTokenService(AppIdentityDbContext context) : IUserTokenService
{
    public IEnumerable<TokenModelDto> GetUserTokens(Func<TokenModelDto, bool> predicate)
    {
        var userTokensDtos = context.UsersTokens
            .Select(ut => new TokenModelDto() 
            { 
                Email = ut.Email, 
                Token = ut.Token, 
                ExpirationDate = ut.ExpirationDate 
            })
            .AsEnumerable()
            .Where(predicate);

        return userTokensDtos;
    }
}