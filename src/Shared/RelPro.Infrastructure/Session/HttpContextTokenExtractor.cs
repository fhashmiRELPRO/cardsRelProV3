using Microsoft.AspNetCore.Http;

namespace RelPro.Infrastructure.Session;

public sealed class HttpContextTokenExtractor : IHttpContextTokenExtractor
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextTokenExtractor(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? CurrentToken
    {
        get
        {
            var ctx = _accessor.HttpContext;
            if (ctx is null) return null;

            var fromHeader = ctx.Request.Headers["userToken"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(fromHeader)) return fromHeader;

            var auth = ctx.Request.Headers.Authorization.ToString();
            if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return auth["Bearer ".Length..].Trim();

            return null;
        }
    }
}
