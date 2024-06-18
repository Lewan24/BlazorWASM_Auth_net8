using System.ComponentModel.DataAnnotations;

namespace App.Modules.Auth.Core.Entities;

public sealed class TokenModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Email { get; set; }
    public string? Token { get; set; }
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddHours(1);
}