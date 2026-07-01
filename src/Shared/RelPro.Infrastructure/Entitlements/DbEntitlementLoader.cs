using Dapper;
using RelPro.Contracts.Dtos;
using RelPro.Infrastructure.Caching;
using RelPro.Infrastructure.Database;

namespace RelPro.Infrastructure.Entitlements;

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
            _ => LoadFromDbAsync(contractId, ct),
            CacheTtl,
            ct);

    private async Task<ContractEntitlementsDto> LoadFromDbAsync(int contractId, CancellationToken ct)
    {
        await using var conn = await _db.OpenAsync(ct);
        var row = await conn.QuerySingleOrDefaultAsync<dynamic>(
            @"SELECT * FROM contract_entitlements
              WHERE  contract_id = @contractId
              ORDER  BY id DESC
              LIMIT  1",
            new { contractId });

        if (row is null)
            return new ContractEntitlementsDto { ContractId = contractId };

        var r = (IDictionary<string, object>)row;
        bool Get(string col) => r.TryGetValue(col, out var v) && Convert.ToBoolean(v);

        // Explicit mapping: legacy contract_entitlements column → new DTO property.
        // Column names are mixed snake_case and camelCase in the DB (legacy inconsistency).
        // Computed flags from the stored proc (allowSalesforce, allowSSO, etc.) are not
        // available here - those require rpro3_read_contract_entitlements_* stored proc.
        // TODO: migrate to stored proc once session validation is refactored to avoid the
        //       extra DB call that stored proc usage would currently require (see ADR note).
        return new ContractEntitlementsDto
        {
            ContractId = contractId,

            ExportToExcel = Get("allow_excel_extract"),
            ExportBulk    = Get("allowListUploads"),

            CRMSalesforce = Get("allowCustomSFDC"),

            DataSocialProfiles = Get("allowLinkedIn"),
            DataTechnographics = Get("allowTechSearch"),

            UserManagement = Get("allowAdminConnectivity"),

            ListCreate = Get("allowListUploads"),
            ListImport = Get("allow_import_list"),

            AISmartSearch     = Get("allowAISearch"),
            AIRecommendations = Get("allowRecommendationEngine"),
            AIInsights        = Get("allowAI"),

            ComplianceGdprTools = Get("gdpr_level"),
        };
    }
}
