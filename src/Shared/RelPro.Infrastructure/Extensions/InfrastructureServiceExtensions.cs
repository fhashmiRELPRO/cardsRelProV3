using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.OpenApi.Models;
using RelPro.Infrastructure.Caching;
using RelPro.Infrastructure.Context;
using RelPro.Infrastructure.Database;
using RelPro.Infrastructure.Email;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Http;
using RelPro.Infrastructure.Logging;
using RelPro.Infrastructure.Middleware;
using RelPro.Infrastructure.OrgServices;
using RelPro.Infrastructure.Session;
using RelPro.Infrastructure.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RelPro.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers all shared infrastructure: RequestContext, cache, DB factories.
    /// Call this in every service's Program.cs.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // RequestContext - scoped so it lives exactly one request
        services.AddScoped<RequestContextHolder>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContextHolder>());

        // Cache
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Registers the legacy session validator (reads CARDS sessions table directly via MySQL)
    /// plus DB-backed contract status and entitlement loaders, all Redis-cached.
    /// Requires IMySqlConnectionFactory and IDistributedCache to be registered first.
    /// </summary>
    public static IServiceCollection AddLegacySessionValidator(this IServiceCollection services)
    {
        services.AddScoped<ILegacySessionDataSource, MySqlLegacySessionDataSource>();
        services.AddScoped<ISessionValidator, LegacySessionValidator>();
        services.AddScoped<IContractStatusLoader, DbContractStatusLoader>();
        services.AddScoped<IEntitlementLoader, DbEntitlementLoader>();

        return services;
    }

    /// <summary>
    /// Registers Redis-backed session store + validators.
    /// Use in services that create or validate their own sessions (no legacy dependency).
    /// </summary>
    public static IServiceCollection AddRedisSessionStore(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ISessionStore, RedisSessionStore>();
        services.AddScoped<IHttpContextTokenExtractor, HttpContextTokenExtractor>();
        services.AddScoped<ISessionValidator, RedisSessionValidator>();
        services.AddScoped<IContractStatusLoader, RedisContractStatusLoader>();
        services.AddScoped<IEntitlementLoader, RedisEntitlementLoader>();

        return services;
    }

    /// <summary>
    /// Registers audit logging (writes to legacy `logs` table) and quota enforcement.
    /// Requires IMySqlConnectionFactory to be registered first.
    /// </summary>
    public static IServiceCollection AddRequestLogging(this IServiceCollection services)
    {
        services.AddScoped<IRequestAuditLogger, DbRequestAuditLogger>();
        services.AddScoped<IQuotaService, DbQuotaService>();
        return services;
    }

    /// <summary>
    /// Registers the SMTP email service for outbound notifications (quota exceeded, login alerts).
    /// Binds EmailOptions from the "Email" config section.
    /// Call BEFORE AddRequestLogging middleware is added to the pipeline.
    /// </summary>
    public static IServiceCollection AddEmailService(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddSingleton<IEmailService, SmtpEmailService>();
        return services;
    }

    /// <summary>
    /// Adds the RequestContextMiddleware to the pipeline.
    /// Must be called AFTER UseRouting and BEFORE MapControllers.
    /// </summary>
    public static IApplicationBuilder UseRequestContext(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestContextMiddleware>();

    /// <summary>
    /// Adds the RequestLoggingMiddleware to the pipeline.
    /// Must be called AFTER UseRequestContext (requires IRequestContext to be populated).
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestLoggingMiddleware>();

    /// <summary>
    /// Registers and wires the global exception handler.
    /// Call this BEFORE UseRouting - it must be the outermost middleware.
    /// </summary>
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app) =>
        app.UseExceptionHandler();

    /// <summary>
    /// Registers the per-tenant org service config repository.
    /// Requires IMySqlConnectionFactory and IDistributedCache to be registered first.
    /// </summary>
    public static IServiceCollection AddUserOrgServiceRepository(this IServiceCollection services)
    {
        services.AddScoped<IUserOrgServiceRepository, MySqlUserOrgServiceRepository>();
        return services;
    }

    /// <summary>
    /// Registers a typed HttpClient for inter-service calls with:
    ///   - Polly standard resilience (retry × 3, circuit-breaker, timeout)
    ///   - Automatic userToken header forwarding from IRequestContext
    ///
    /// Usage in Program.cs:
    ///   builder.Services.AddResilientHttpClient&lt;IMyClient, MyClient&gt;(
    ///       baseAddress: "http://user-service:5020");
    /// </summary>
    /// <summary>
    /// Registers Swagger/OpenAPI with the standard RelPro security definition and response filter.
    /// Call this in every service's Program.cs instead of raw AddSwaggerGen().
    ///
    /// Usage:
    ///   builder.Services.AddSwaggerWithAuth("RelPro User Service", "Manages user profiles.", "RelPro.User.Api.xml");
    ///   // In app pipeline (Development only):
    ///   app.UseSwaggerWithDocs("RelPro User Service");
    /// </summary>
    public static IServiceCollection AddSwaggerWithAuth(
        this IServiceCollection services,
        string title,
        string description,
        string xmlDocFileName)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = title,
                Version = "v1",
                Description = $"{description}\n\n" +
                    "---\n\n" +
                    "**Internal Service API** - intended for service-to-service and developer use.\n\n" +
                    "Enterprise (customer-facing) endpoints are documented in the API Gateway Swagger.\n\n" +
                    "**Error envelope** - all error responses use `{ success: false, errorCode: \"CODE\", errorMessage: \"...\" }`."
            });

            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlDocFileName);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);

            c.AddSecurityDefinition("userToken", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "userToken",
                Description = "Session token - obtained from `POST /v1/auth/login` or copied from the CARDS legacy application cookie (`userToken`)."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "userToken" }
                    },
                    []
                }
            });

            c.OperationFilter<StandardResponseOperationFilter>();
        });

        return services;
    }

    /// <summary>
    /// Enables Swagger UI at /swagger. Call inside if (app.Environment.IsDevelopment()).
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithDocs(this IApplicationBuilder app, string apiTitle)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiTitle} v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = apiTitle;
            c.DefaultModelsExpandDepth(-1); // hide schema panel by default
        });
        return app;
    }

    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImpl>(
        this IServiceCollection services,
        string baseAddress)
        where TClient : class
        where TImpl    : class, TClient
    {
        services.AddScoped<TokenForwardingHandler>();

        var clientBuilder = services
            .AddHttpClient<TClient, TImpl>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
            })
            .AddHttpMessageHandler<TokenForwardingHandler>();

        clientBuilder.AddStandardResilienceHandler(opts =>
        {
            opts.Retry.MaxRetryAttempts       = 3;
            opts.Retry.UseJitter              = true;
            opts.TotalRequestTimeout.Timeout  = TimeSpan.FromSeconds(30);
        });

        return clientBuilder;
    }
}
