using Dapper;
using RelPro.Contracts.Dtos;
using RelPro.Infrastructure.Caching;
using RelPro.Infrastructure.Database;
using RelPro.Infrastructure.Entitlements;

namespace RelPro.CARDS.Api.Auth.Infrastructure;

public sealed class DbEntitlementLoader : IEntitlementLoader
{
    private readonly IMySqlConnectionFactory _db;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(20);

    public DbEntitlementLoader(IMySqlConnectionFactory db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public Task<ContractEntitlementsDto> LoadAsync(int contractId, CancellationToken ct = default) =>
        _cache.GetOrSetAsync(
            $"entitlements:{contractId}",
            _ => LoadFromDbAsync(contractId),
            CacheTtl,
            ct);

    private async Task<ContractEntitlementsDto> LoadFromDbAsync(int contractId)
    {
        await using var conn = await _db.OpenAsync();
        var row = await conn.QuerySingleOrDefaultAsync<dynamic>(
            "SELECT * FROM contract_entitlements WHERE contract_id = @contractId",
            new { contractId });

        if (row is null)
            return new ContractEntitlementsDto { ContractId = contractId };

        var dto = new ContractEntitlementsDto { ContractId = contractId };
        var dtoType = typeof(ContractEntitlementsDto);
        var rowDict = (IDictionary<string, object>)row;

        foreach (var (col, val) in rowDict)
        {
            var prop = dtoType.GetProperty(ToPascalCase(col));
            if (prop?.CanWrite == true && prop.PropertyType == typeof(bool))
                prop.SetValue(dto, Convert.ToBoolean(val));
        }

        return dto;
    }

    private static string ToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase)) return snakeCase;
        return string.Concat(snakeCase.Split('_')
            .Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
    }
}
