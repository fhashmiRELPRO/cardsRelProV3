using RelPro.Auth.Api.Options;
using RelPro.Auth.Api.Repositories;
using RelPro.Auth.Api.Services;
using RelPro.Infrastructure.Configuration;
using RelPro.Infrastructure.Database;
using RelPro.Infrastructure.Extensions;
using RelPro.Infrastructure.Session;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEncryptedConfiguration();


builder.Services.AddControllers();
builder.Services.AddGlobalExceptionHandler();

// Shared infrastructure: IRequestContext, ICacheService, scoped request context holder
builder.Services.AddInfrastructure();

// Redis distributed cache
builder.Services.AddStackExchangeRedisCache(opts =>
{
    opts.Configuration = builder.Configuration["Redis:ConnectionString"]
        ?? throw new InvalidOperationException("Redis:ConnectionString is required");
});

// Redis-backed session store - keeps ISessionStore for LoginService.CreateAsync()
builder.Services.AddRedisSessionStore();

// MySQL connection factory
builder.Services.Configure<MySqlOptions>(builder.Configuration.GetSection("MySql"));
builder.Services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();

// Legacy session validator overrides the Redis validator registered above.
// ISessionStore from AddRedisSessionStore() is still available for LoginService.
builder.Services.Configure<LegacySessionOptions>(builder.Configuration.GetSection("LegacySession"));
builder.Services.AddLegacySessionValidator();

// Audit logging + quota enforcement (writes to legacy logs/user_quotas tables)
builder.Services.AddRequestLogging();

// Outbound email (login-failure alerts)
builder.Services.AddEmailService(builder.Configuration);

// Login failure lockout (Redis-backed sliding window counter)
builder.Services.Configure<ContractOptions>(builder.Configuration.GetSection("Contracts"));
builder.Services.AddScoped<ILoginLockoutService, RedisLoginLockoutService>();

// Auth domain services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ILoginService, LoginService>();

builder.Services.AddSwaggerWithAuth(
    title: "RelPro Auth Service",
    description: "Session management. During the CARDS migration, the primary token source is the CARDS legacy session store - " +
                 "`POST /v1/auth/login` is used for development/testing only and creates a Redis-backed session.",
    xmlDocFileName: "RelPro.Auth.Api.xml");

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwaggerWithDocs("RelPro Auth Service");

app.UseGlobalExceptionHandler();
app.UseRouting();
app.UseRequestContext();
app.UseRequestLogging();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
