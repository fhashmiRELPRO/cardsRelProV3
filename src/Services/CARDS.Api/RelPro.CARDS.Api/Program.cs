using RelPro.CARDS.Api.Auth.Options;
using RelPro.CARDS.Api.Auth.Services;
using RelPro.CARDS.Api.Search.Repositories;
using RelPro.CARDS.Api.User.Services;
using RelPro.Infrastructure.Configuration;
using RelPro.Infrastructure.Database;
using RelPro.Infrastructure.Extensions;
using RelPro.Infrastructure.Session;

// Fully-qualified aliases resolve the two IUserRepository interfaces (auth vs user-profile).
using IAuthUserRepository = RelPro.CARDS.Api.Auth.Repositories.IUserRepository;
using AuthUserRepository = RelPro.CARDS.Api.Auth.Repositories.UserRepository;
using IUserProfileRepository = RelPro.CARDS.Api.User.Repositories.IUserRepository;
using UserProfileRepository = RelPro.CARDS.Api.User.Repositories.MySqlUserRepository;

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

builder.Services.AddRedisSessionStore();

builder.Services.Configure<MySqlOptions>(builder.Configuration.GetSection("MySql"));
builder.Services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();

builder.Services.Configure<LegacySessionOptions>(builder.Configuration.GetSection("LegacySession"));
builder.Services.AddLegacySessionValidator();

builder.Services.AddRequestLogging();

builder.Services.AddEmailService(builder.Configuration);

builder.Services.Configure<ContractOptions>(builder.Configuration.GetSection("Contracts"));
builder.Services.AddScoped<ILoginLockoutService, RedisLoginLockoutService>();
builder.Services.AddScoped<IAuthUserRepository, AuthUserRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ILoginService, LoginService>();

builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IMongoClientFactory, MongoClientFactory>();
builder.Services.AddUserOrgServiceRepository();
builder.Services.AddScoped<IPersonSearchRepository, PersonSearchRepository>();
builder.Services.Configure<ProspectorSearchOptions>(builder.Configuration.GetSection("ProspectorSearch"));
builder.Services.AddScoped<IProspectorSearchRepository, ProspectorSearchRepository>();

builder.Services.AddSwaggerWithAuth(
    title: "RelPro CARDS API",
    description: "CARDS migration API — Auth, User, and Search in one deployable service. " +
                 "Production sessions from CARDS legacy are accepted via the userToken header. " +
                 "POST /v1/auth/login is for development/testing only.",
    xmlDocFileName: "RelPro.CARDS.Api.xml");

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwaggerWithDocs("RelPro CARDS API");

app.UseGlobalExceptionHandler();
app.UseRouting();
app.UseRequestContext();
app.UseRequestLogging();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
