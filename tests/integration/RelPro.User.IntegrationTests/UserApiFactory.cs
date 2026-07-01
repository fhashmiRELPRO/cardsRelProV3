using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RelPro.Infrastructure.Email;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Logging;
using RelPro.Infrastructure.Session;
using RelPro.Infrastructure.Testing;
using RelPro.User.Api.Repositories;
using RelPro.User.IntegrationTests.Stubs;

namespace RelPro.User.IntegrationTests;

/// <summary>
/// Hosts the User.Service in-process with all external dependencies stubbed out.
/// Tests inherit from this or create it via IClassFixture.
/// </summary>
public sealed class UserApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace Redis with an in-memory cache so no Redis connection is needed.
            services.RemoveAll<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            services.AddDistributedMemoryCache();

            // Replace live session/contract/entitlement infrastructure with stubs.
            services.RemoveAll<ISessionValidator>();
            services.RemoveAll<IContractStatusLoader>();
            services.RemoveAll<IEntitlementLoader>();
            services.AddSingleton<ISessionValidator, TokenAwareSessionValidator>();
            services.AddSingleton<IContractStatusLoader, StubContractStatusLoader>();
            services.AddSingleton<IEntitlementLoader, StubEntitlementLoader>();

            // Replace live MySQL repository with the in-memory stub.
            services.RemoveAll<IUserRepository>();
            services.AddSingleton<IUserRepository, StubUserRepository>();

            // Replace live audit logging, quota, and email with no-ops (no external deps in tests).
            services.RemoveAll<IRequestAuditLogger>();
            services.RemoveAll<IQuotaService>();
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IRequestAuditLogger, NoOpAuditLogger>();
            services.AddSingleton<IQuotaService, NoOpQuotaService>();
            services.AddSingleton<IEmailService, NoOpEmailService>();
        });
    }
}
