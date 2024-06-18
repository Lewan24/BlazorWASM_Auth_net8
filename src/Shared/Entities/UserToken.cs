namespace Shared.Entities;

public class UserToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? UserName { get; set; }
    public string? Token { get; set; }
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddMinutes(20);
    public bool IsInActive { get; set; } = false;
}
