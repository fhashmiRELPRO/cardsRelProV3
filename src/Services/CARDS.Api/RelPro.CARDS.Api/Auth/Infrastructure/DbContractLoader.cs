using Dapper;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Caching;
using RelPro.Infrastructure.Database;
using RelPro.Infrastructure.Entitlements;

namespace RelPro.CARDS.Api.Auth.Infrastructure;

public sealed class DbContractLoader : IContractStatusLoader
{
    private readonly IMySqlConnectionFactory _db;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public DbContractLoader(IMySqlConnectionFactory db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ContractStatusInfo?> LoadAsync(int contractId, CancellationToken ct = default)
    {
        var cached = await _cache.GetAsync<ContractStatusInfo>($"contract:status:{contractId}", ct);
        if (cached is not null)
            return cached;

        var info = await LoadFromDbAsync(contractId);
        if (info is not null)
            await _cache.SetAsync($"contract:status:{contractId}", info, CacheTtl, ct);

        return info;
    }

    private async Task<ContractStatusInfo?> LoadFromDbAsync(int contractId)
    {
        await using var conn = await _db.OpenAsync();
        var row = await conn.QuerySingleOrDefaultAsync(
            @"SELECT contract_id, status, expiry_date
              FROM contracts
              WHERE contract_id = @contractId",
            new { contractId });

        if (row is null) return null;

        var status = Enum.TryParse((string?)row.status, ignoreCase: true, out ContractStatus s)
            ? s
            : ContractStatus.Unknown;

        DateOnly? expiry = row.expiry_date is not null
            ? DateOnly.FromDateTime((DateTime)row.expiry_date)
            : null;

        var isActive = status == ContractStatus.Active
            && (expiry is null || expiry >= DateOnly.FromDateTime(DateTime.UtcNow));

        return new ContractStatusInfo(contractId, status, expiry, isActive);
    }
}
