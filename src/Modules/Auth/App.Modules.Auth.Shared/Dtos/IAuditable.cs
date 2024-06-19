namespace App.Models.Auth.Shared.Dtos;

internal interface IAuditable
{
    Guid Gid { get; set; }
}