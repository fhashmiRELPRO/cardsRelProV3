using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RelPro.Common.Exceptions;
using RelPro.Contracts.Common;
using System.Net;
using System.Text.Json;

namespace RelPro.Infrastructure.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        var (statusCode, errorCode, message) = MapException(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        else
            _logger.LogWarning("Handled exception {Type} on {Method} {Path}: {Code}",
                exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, errorCode);

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(ApiResponse.Fail(errorCode, message));
        await httpContext.Response.WriteAsync(body, ct);

        return true;
    }

    private static (HttpStatusCode status, string code, string message) MapException(Exception ex) =>
        ex switch
        {
            ResourceNotFoundException e =>
                (HttpStatusCode.NotFound, "NOT_FOUND", e.Message),

            EntitlementException e =>
                (HttpStatusCode.Forbidden, "ENTITLEMENT_REQUIRED", e.Message),

            ContractInactiveException e =>
                (HttpStatusCode.Forbidden, "CONTRACT_INACTIVE", e.Message),

            QuotaExceededException e =>
                (HttpStatusCode.TooManyRequests, "QUOTA_EXCEEDED", e.Message),

            ServiceUnavailableException e =>
                (HttpStatusCode.ServiceUnavailable, "SERVICE_UNAVAILABLE", e.Message),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Authentication required."),

            // OperationCanceledException covers both manual cancellation AND HttpClient timeouts
            OperationCanceledException =>
                (HttpStatusCode.RequestTimeout, "REQUEST_TIMEOUT",
                    "The request timed out. Please try again."),

            TimeoutException =>
                (HttpStatusCode.RequestTimeout, "REQUEST_TIMEOUT",
                    "The operation timed out. Please try again."),

            ArgumentException e =>
                (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", e.Message),

            _ =>
                (HttpStatusCode.InternalServerError, "INTERNAL_ERROR",
                    "An unexpected error occurred. Please try again or contact support.")
        };
}
