using Microsoft.Extensions.Caching.Distributed;
using RelPro.Infrastructure.Database;
using System.Text.Json;

namespace RelPro.Infrastructure.OrgServices;

public sealed class MySqlUserOrgServiceRepository : BaseMySqlRepository, IUserOrgServiceRepository
{
    private readonly IDistributedCache _cache;

    private static readonly DistributedCacheEntryOptions CacheOpts = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    public MySqlUserOrgServiceRepository(IMySqlConnectionFactory db, IDistributedCache cache)
        : base(db) => _cache = cache;

    public async Task<UserOrgServiceRecord?> GetAsync(int orgId, int dataSourceId, CancellationToken ct = default)
    {
        var cacheKey = $"org-services:{orgId}";

        var cached = await _cache.GetStringAsync(cacheKey, ct);

        List<UserOrgServiceRecord> all;

        if (cached is not null)
        {
            all = JsonSerializer.Deserialize<List<UserOrgServiceRecord>>(cached) ?? [];
        }
        else
        {
            var rows = await CallProcedureAsync<UserOrgServiceRecord>(
                "rcp2_read_org_services", new { org_id = orgId }, ct);

            all = rows.ToList();

            await _cache.SetStringAsync(
                cacheKey, JsonSerializer.Serialize(all), CacheOpts, ct);
        }

        return all.FirstOrDefault(s => s.DataSourceId == dataSourceId);
    }
}
