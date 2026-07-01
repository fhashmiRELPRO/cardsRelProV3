using Dapper;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Caching;
using RelPro.Infrastructure.Database;

namespace RelPro.Infrastructure.Entitlements;

public sealed class DbContractStatusLoader : IContractStatusLoader
{
    private readonly IMySqlConnectionFactory _db;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public DbContractStatusLoader(IMySqlConnectionFactory db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ContractStatusInfo?> LoadAsync(int contractId, CancellationToken ct = default)
    {
        var cached = await _cache.GetAsync<ContractStatusInfo>($"contract:status:{contractId}", ct);
        if (cached is not null) return cached;

        var info = await LoadFromDbAsync(contractId, ct);
        if (info is not null)
            await _cache.SetAsync($"contract:status:{contractId}", info, CacheTtl, ct);

        return info;
    }

    private async Task<ContractStatusInfo?> LoadFromDbAsync(int contractId, CancellationToken ct)
    {
        await using var conn = await _db.OpenAsync(ct);
        var row = await conn.QuerySingleOrDefaultAsync<ContractRow>(
            @"SELECT status_id AS StatusId, end_date AS EndDate
              FROM   contracts
              WHERE  id          = @contractId
                AND  deleted_flag = 0",
            new { contractId });

        if (row is null) return null;

        var status = Enum.IsDefined(typeof(ContractStatus), row.StatusId)
            ? (ContractStatus)row.StatusId
            : ContractStatus.Unknown;

        DateOnly? expiry = row.EndDate.HasValue
            ? DateOnly.FromDateTime(row.EndDate.Value)
            : null;

        // Active when not past end_date - status_id alone is unreliable on staging
        var isActive = expiry is null || expiry >= DateOnly.FromDateTime(DateTime.UtcNow);

        return new ContractStatusInfo(contractId, status, expiry, isActive);
    }

    private sealed class ContractRow
    {
        public int StatusId { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
