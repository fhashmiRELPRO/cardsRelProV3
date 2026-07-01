using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RelPro.Contracts.Common;
using RelPro.Infrastructure.Context;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Session;
using System.Net;
using System.Text.Json;

namespace RelPro.Infrastructure.Middleware;

public sealed class RequestContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestContextMiddleware> _logger;

    public RequestContextMiddleware(RequestDelegate next, ILogger<RequestContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        RequestContextHolder holder,
        ISessionValidator sessionValidator,
        IEntitlementLoader entitlementLoader,
        IContractStatusLoader contractStatusLoader)
    {
        var token = httpContext.Request.Headers["userToken"].FirstOrDefault()
                    ?? ExtractBearerToken(httpContext.Request.Headers.Authorization.ToString());

        if (string.IsNullOrWhiteSpace(token))
        {
            // Allow endpoints marked [AllowAnonymous] to proceed without a session
            if (IsAnonymousEndpoint(httpContext))
            {
                await _next(httpContext);
                return;
            }

            await WriteError(httpContext, HttpStatusCode.Unauthorized, "MISSING_TOKEN", "Authentication token is required.");
            return;
        }

        var session = await sessionValidator.ValidateAsync(token, httpContext.RequestAborted);
        if (session is null)
        {
            await WriteError(httpContext, HttpStatusCode.Unauthorized, "INVALID_SESSION", "Session is invalid or expired.");
            return;
        }

        var contract = await contractStatusLoader.LoadAsync(session.ContractId, httpContext.RequestAborted);
        if (contract is null)
        {
            _logger.LogWarning("Contract {ContractId} not found for user {UserId}", session.ContractId, session.UserId);
            await WriteError(httpContext, HttpStatusCode.Forbidden, "CONTRACT_NOT_FOUND", "Contract not found.");
            return;
        }

        if (!contract.IsActive)
        {
            await WriteError(httpContext, HttpStatusCode.Forbidden, "CONTRACT_INACTIVE", "Contract is not active.");
            return;
        }

        var entitlements = await entitlementLoader.LoadAsync(session.ContractId, httpContext.RequestAborted);

        holder.Populate(token, session, contract, entitlements);

        await _next(httpContext);
    }

    private static bool IsAnonymousEndpoint(HttpContext httpContext)
    {
        if (httpContext.Request.Path.StartsWithSegments("/health"))
            return true;
        var endpoint = httpContext.GetEndpoint();
        return endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is not null;
    }

    private static string? ExtractBearerToken(string? authHeader)
    {
        if (string.IsNullOrWhiteSpace(authHeader)) return null;
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return authHeader["Bearer ".Length..].Trim();
        return null;
    }

    private static Task WriteError(HttpContext ctx, HttpStatusCode status, string code, string message)
    {
        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(ApiResponse.Fail(code, message));
        return ctx.Response.WriteAsync(body);
    }
}
