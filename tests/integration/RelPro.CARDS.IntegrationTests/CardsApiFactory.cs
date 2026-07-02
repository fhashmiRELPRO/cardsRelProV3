using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RelPro.CARDS.Api.Auth.Repositories;
using RelPro.CARDS.Api.Search.Repositories;
using RelPro.Infrastructure.Email;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Logging;
using RelPro.Infrastructure.Session;
using RelPro.Infrastructure.Testing;
using RelPro.CARDS.IntegrationTests.Stubs;

using IAuthUserRepository = RelPro.CARDS.Api.Auth.Repositories.IUserRepository;
using IUserProfileRepository = RelPro.CARDS.Api.User.Repositories.IUserRepository;

namespace RelPro.CARDS.IntegrationTests;

public sealed class CardsApiFactory : WebApplicationFactory<Program>
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

            services.RemoveAll<IUserProfileRepository>();
            services.AddSingleton<IUserProfileRepository, StubUserRepository>();

            services.RemoveAll<IPersonSearchRepository>();
            services.AddSingleton<IPersonSearchRepository, StubPersonSearchRepository>();

            services.RemoveAll<IProspectorSearchRepository>();
            services.AddSingleton<IProspectorSearchRepository, StubProspectorSearchRepository>();

            services.RemoveAll<IRequestAuditLogger>();
            services.RemoveAll<IQuotaService>();
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IRequestAuditLogger, NoOpAuditLogger>();
            services.AddSingleton<IQuotaService, NoOpQuotaService>();
            services.AddSingleton<IEmailService, NoOpEmailService>();
        });
    }
}
