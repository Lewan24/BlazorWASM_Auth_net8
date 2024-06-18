namespace Shared.Attributes;

/// <summary>
/// Allows method to be executed without the token validation proccess
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public abstract class AllowWithoutTokenValidationAttribute : Attribute
{
}