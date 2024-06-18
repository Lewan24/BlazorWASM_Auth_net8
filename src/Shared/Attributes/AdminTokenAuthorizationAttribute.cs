using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using OneOf.Types;

namespace Shared.Attributes;

/// <summary>
/// Same as <see cref="BasicTokenAuthorizationAttribute"/> but with additional validation if user is in admin role
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class AdminTokenAuthorizationAttribute : BasicTokenAuthorizationAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = CheckIfAnonymousAndValidateToken(context);
        if (!result.IsT0)
        {
            context.Result = new BadRequestObjectResult(result.AsT1);
            return;
        }

        // TODO: Implement administration module to handle this
        // var isAdmin = await IsUserAdmin(context);
        // if (isAdmin.IsT1)
        // {
        //     context.Result = new BadRequestObjectResult("Action requires administration authorization");
        //     return;
        // }

        await next();
    }

    // private async Task<OneOf<Yes, No>> IsUserAdmin(ActionExecutingContext context)
    // {
    //     var adminApi = context.HttpContext.RequestServices.GetService<IAdminModuleApi>();
    //
    //     if (adminApi is null)
    //         return new No();
    //
    //     var isAdmin = await adminApi.IsUserAdminAsync(context.HttpContext.User.Identity?.Name);
    //
    //     if (isAdmin.IsT1)
    //         return new No();
    //
    //     return new Yes();
    // }
}