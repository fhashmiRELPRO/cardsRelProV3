using System.Runtime.ExceptionServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RelPro.Contracts.Common;
using RelPro.Infrastructure.Context;
using RelPro.Infrastructure.Email;
using RelPro.Infrastructure.Logging;

namespace RelPro.Infrastructure.Middleware;

/// <summary>
/// Runs AFTER RequestContextMiddleware. For every authenticated request:
///   1. Pre-flight quota check  - returns 429 if the user's limit is exhausted
///   2. Awaits the downstream pipeline
///   3. Writes an audit log entry to the legacy `logs` table
///   4. Increments the user's gross quota counter (on success only, HTTP &lt; 400)
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        IRequestContext ctx,
        IRequestAuditLogger auditLogger,
        IQuotaService quotaService,
        IEmailService emailService)
    {
        // Skip logging/quota for unauthenticated endpoints (e.g. /v1/auth/login).
        if (!ctx.IsPopulated)
        {
            await _next(httpContext);
            return;
        }

        // --- 1. Quota pre-check ---
        var allowed = await quotaService.CheckAsync(ctx.UserId, httpContext.RequestAborted);
        if (!allowed)
        {
            httpContext.Response.StatusCode  = StatusCodes.Status429TooManyRequests;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(
                    ApiResponse.Fail("QUOTA_EXCEEDED",
                        "Request quota exhausted. Contact your administrator to increase your limit.")),
                CancellationToken.None);

            try
            {
                await emailService.SendQuotaExceededAsync(
                    ctx.UserEmail ?? string.Empty,
                    ctx.UserName  ?? string.Empty,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Quota exceeded email failed for user {UserId}", ctx.UserId);
            }

            return;
        }

        // --- 2. Execute downstream pipeline ---
        var startTime = DateTime.UtcNow;
        Exception? captured = null;

        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            captured = ex;
        }

        // --- 3. Audit log (always, regardless of success/failure) ---
        var endTime    = DateTime.UtcNow;
        var statusCode = captured is not null ? 500 : httpContext.Response.StatusCode;
        var state      = statusCode >= 400 ? -statusCode : 0;

        var entry = new AuditLogEntry
        {
            UserId          = ctx.UserId,
            Method          = httpContext.Request.Path.ToString(),
            ObjectType      = DeriveObjectType(httpContext.Request.Path),
            Verb            = httpContext.Request.Method,
            State           = state,
            ErrorMessage    = captured?.Message ?? (statusCode >= 400 ? $"HTTP {statusCode}" : null),
            Duration        = (float)(endTime - startTime).TotalMilliseconds,
            StartTime       = startTime,
            EndTime         = endTime,
            Logged          = 1,
            QueryString     = httpContext.Request.QueryString.Value,
            DataSourceId    = ctx.DataSourceId,
            UserEmail       = ctx.UserEmail,
            UserNonce       = ctx.SessionToken,
            UserIp          = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserPort        = httpContext.Connection.RemotePort,
        };

        try
        {
            await auditLogger.LogAsync(entry, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit log failed for user {UserId} {Verb} {Path}",
                ctx.UserId, entry.Verb, entry.Method);
        }

        // --- 4. Quota increment (success only) ---
        if (statusCode < 400)
        {
            try
            {
                await quotaService.IncrementAsync(ctx.UserId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Quota increment failed for user {UserId}", ctx.UserId);
            }
        }

        // Re-throw captured exception so GlobalExceptionHandler can handle it
        if (captured is not null)
            ExceptionDispatchInfo.Capture(captured).Throw();
    }

    private static string DeriveObjectType(PathString path)
    {
        var segments = path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);
        // /v1/user/me  → "user"
        // /v1/search   → "search"
        return segments.Length >= 2 ? segments[1] : "api";
    }
}
