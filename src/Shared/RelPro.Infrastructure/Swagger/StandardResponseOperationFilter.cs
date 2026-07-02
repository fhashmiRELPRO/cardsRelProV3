using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace RelPro.Infrastructure.Swagger;

public sealed class StandardResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous =
            context.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() is not null
            || (context.MethodInfo.DeclaringType?.GetCustomAttribute<AllowAnonymousAttribute>() is not null);

        if (!hasAllowAnonymous)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "**Unauthorized** - `MISSING_TOKEN`: no token supplied. `INVALID_SESSION`: token is expired or invalid."
            });
        }

        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "**Internal Server Error** - `INTERNAL_ERROR`: unexpected failure; contact support."
        });
    }
}
