using System.ComponentModel.DataAnnotations;
using App.Models.Auth.Shared.Dtos;

namespace App.Models.Auth.Shared.Entities.Tokens;

public sealed class TokenModelDto
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddHours(1);
}