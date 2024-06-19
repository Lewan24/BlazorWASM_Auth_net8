using OneOf;
using OneOf.Types;

namespace App.Models.Auth.Shared.Interfaces.Health;

public interface IAuthHealthService
{
    Task<OneOf<True, False,  Error>>  CheckHealthAsync();
}