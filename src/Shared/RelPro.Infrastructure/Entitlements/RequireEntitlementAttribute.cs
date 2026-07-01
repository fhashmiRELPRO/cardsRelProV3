using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using RelPro.Contracts.Common;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Context;

namespace RelPro.Infrastructure.Entitlements;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireEntitlementAttribute : Attribute, IAsyncActionFilter
{
    private readonly EntitlementFeature _feature;

    public RequireEntitlementAttribute(EntitlementFeature feature)
    {
        _feature = feature;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var requestContext = context.HttpContext.RequestServices.GetRequiredService<IRequestContext>();

        if (!requestContext.HasEntitlement(_feature))
        {
            context.Result = new ObjectResult(
                ApiResponse.Fail("ENTITLEMENT_REQUIRED", $"This action requires the '{_feature}' entitlement."))
            {
                StatusCode = 403
            };
            return;
        }

        await next();
    }
}
