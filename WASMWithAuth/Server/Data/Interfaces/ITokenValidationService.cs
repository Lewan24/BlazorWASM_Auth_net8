using WASMWithAuth.Shared.Entities;

namespace WASMWithAuth.Server.Data.Interfaces;

public interface ITokenValidationService
{
    Task<bool> CheckValidation(string token, string userName);
    Task<bool> CheckExpiration(UserToken userToken);
}