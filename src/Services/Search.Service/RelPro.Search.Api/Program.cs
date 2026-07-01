using RelPro.Infrastructure.Configuration;
using RelPro.Infrastructure.Database;
using RelPro.Infrastructure.Extensions;
using RelPro.Infrastructure.Session;
using RelPro.Search.Api.Repositories;

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

// MySQL for person/company search queries
builder.Services.Configure<MySqlOptions>(builder.Configuration.GetSection("MySql"));
builder.Services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();

// Legacy session validator - validates tokens issued by CARDS (reads sessions table directly)
builder.Services.Configure<LegacySessionOptions>(builder.Configuration.GetSection("LegacySession"));
builder.Services.AddLegacySessionValidator();

// Audit logging + quota enforcement (writes to legacy logs/user_quotas tables)
builder.Services.AddRequestLogging();

// Outbound email (quota-exceeded notifications)
builder.Services.AddEmailService(builder.Configuration);

// MongoDB - people/individual search via Atlas Search
builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IMongoClientFactory, MongoClientFactory>();

// Per-tenant org service config (database/collection names loaded from user_org_services at runtime)
builder.Services.AddUserOrgServiceRepository();

// Repositories
builder.Services.AddScoped<IPersonSearchRepository, PersonSearchRepository>();
builder.Services.Configure<ProspectorSearchOptions>(builder.Configuration.GetSection("ProspectorSearch"));
builder.Services.AddScoped<IProspectorSearchRepository, ProspectorSearchRepository>();

builder.Services.AddSwaggerWithAuth(
    title: "RelPro Search Service",
    description: "People and company search powered by MySQL full-text and MongoDB Atlas Search. " +
                 "The `GET /v1/search` endpoint is wire-compatible with the CARDS legacy `/prospector/v1/search` - " +
                 "frontend clients can point here with no response-format changes.",
    xmlDocFileName: "RelPro.Search.Api.xml");

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwaggerWithDocs("RelPro Search Service");

app.UseGlobalExceptionHandler();
app.UseRouting();
app.UseRequestContext();
app.UseRequestLogging();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
