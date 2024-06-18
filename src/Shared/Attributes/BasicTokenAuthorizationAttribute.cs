using App.Models.Auth.Shared.Interfaces.Token;
using App.Models.Auth.Shared.Static.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OneOf;
using OneOf.Types;

namespace Shared.Attributes;

/// <summary>
/// Specifies that the class or method needs to be validated with token directed in auth header before request
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class BasicTokenAuthorizationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var result = CheckIfAnonymousAndValidateToken(context);
        if (!result.IsT0)
        {
            context.Result = new BadRequestObjectResult(result.AsT1);
            return;
        }

        base.OnActionExecuting(context);
    }

    protected OneOf<Success, string> CheckIfAnonymousAndValidateToken(ActionExecutingContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowWithoutTokenValidationAttribute>().Any();

        if (!allowAnonymous)
        {
            var validationResult = IsTokenValid(context);

            if (validationResult.IsT1)
                return validationResult.AsT1;
        }

        return new Success();
    }

    private OneOf<Success, string> IsTokenValid(ActionExecutingContext context)
    {
        var authToken = context.HttpContext.Request.Headers[AuthHeaderName.Name].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authToken))
            return "Invalid Token";

        var tokenValidationService = (ITokenValidationService?)context.HttpContext.RequestServices.GetService(typeof(ITokenValidationService));
        if (tokenValidationService is null)
            return "Can't retrieve required service for token validation";

        var isTokenValid = tokenValidationService.IsValid(authToken, context.HttpContext.User.Identity?.Name);

        return isTokenValid.Match<OneOf<Success, string>>
        (
            valid => new Success(),
            invalid => "Invalid Token"
        );
    }
}