namespace App.Server.Data.Interfaces;

public interface ITokenValidationService
{
    bool CheckValidation(string token, string userName);
}