namespace WASMWithAuth.Server.Data.Interfaces;

public interface ITokenValidationService
{
    Task<bool> CheckValidation(string token, string userName);
    Task<bool> CheckExpiration(string token);
}