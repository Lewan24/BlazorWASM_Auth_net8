using App.Models.Auth.Shared.Entities.Tokens;

namespace App.Modules.Auth.Infrastructure.Repositories;

public interface IUserTokenService
{
    IEnumerable<TokenModelDto> GetUserTokens(Func<TokenModelDto, bool> predicate);
}