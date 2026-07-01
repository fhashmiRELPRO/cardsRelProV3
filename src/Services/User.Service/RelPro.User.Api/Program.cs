using RelPro.Infrastructure.Configuration;
using RelPro.Infrastructure.Database;
using RelPro.Infrastructure.Extensions;
using RelPro.Infrastructure.Session;
using RelPro.User.Api.Repositories;
using RelPro.User.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEncryptedConfiguration();


builder.Services.AddControllers();
builder.Services.AddGlobalExceptionHandler();

builder.Services.AddInfrastructure();

builder.Services.AddStackExchangeRedisCache(opts =>
{
    opts.Configuration = builder.Configuration["Redis:ConnectionString"]
        ?? throw new InvalidOperationException("Redis:ConnectionString is required");
});

// MySQL connection factory
builder.Services.Configure<MySqlOptions>(builder.Configuration.GetSection("MySql"));
builder.Services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();

// Legacy session validator - validates tokens issued by CARDS (reads sessions table directly)
builder.Services.Configure<LegacySessionOptions>(builder.Configuration.GetSection("LegacySession"));
builder.Services.AddLegacySessionValidator();

// Audit logging + quota enforcement (writes to legacy logs/user_quotas tables)
builder.Services.AddRequestLogging();

// Outbound email (quota-exceeded notifications)
builder.Services.AddEmailService(builder.Configuration);

// User repositories and services
builder.Services.AddScoped<IUserRepository, MySqlUserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSwaggerWithAuth(
    title: "RelPro User Service",
    description: "Manages user profiles, roles, and user administration. " +
                 "Validates sessions via the CARDS legacy session store during the strangler-fig migration.",
    xmlDocFileName: "RelPro.User.Api.xml");

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwaggerWithDocs("RelPro User Service");

app.UseGlobalExceptionHandler();
app.UseRouting();
app.UseRequestContext();
app.UseRequestLogging();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
