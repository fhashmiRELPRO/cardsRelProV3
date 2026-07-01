using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RelPro.Infrastructure.Email;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Logging;
using RelPro.Infrastructure.Session;
using RelPro.Infrastructure.Testing;
using RelPro.Search.Api.Repositories;
using RelPro.Search.IntegrationTests.Stubs;

namespace RelPro.Search.IntegrationTests;

/// <summary>
/// Hosts the Search.Service in-process with all external dependencies stubbed out.
/// </summary>
public sealed class SearchApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            services.AddDistributedMemoryCache();

            services.RemoveAll<ISessionValidator>();
            services.RemoveAll<IContractStatusLoader>();
            services.RemoveAll<IEntitlementLoader>();
            services.AddSingleton<ISessionValidator, TokenAwareSessionValidator>();
            services.AddSingleton<IContractStatusLoader, StubContractStatusLoader>();
            services.AddSingleton<IEntitlementLoader, StubEntitlementLoader>();

            services.RemoveAll<IPersonSearchRepository>();
            services.AddSingleton<IPersonSearchRepository, StubPersonSearchRepository>();

            services.RemoveAll<IProspectorSearchRepository>();
            services.AddSingleton<IProspectorSearchRepository, StubProspectorSearchRepository>();

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
