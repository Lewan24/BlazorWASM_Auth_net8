namespace App.Models.Auth.Api.Entities;

internal sealed record AuthSettings
{
    public bool AutoConfirmAccount { get; init; }
    public int DefaultTokenExpirationTimeInMinutes { get; init; } = 30;
    public bool EnableRegisterModule { get; init; }
    public string? MainAdminEmailAddress { get; init; }
}