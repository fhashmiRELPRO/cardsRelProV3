using RelPro.Contracts.Dtos;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Session;

namespace RelPro.Infrastructure.Context;

public sealed class RequestContextHolder : IRequestContext
{
    private SessionValidationResult? _session;
    private ContractStatusInfo? _contract;
    private ContractEntitlementsDto? _entitlements;
    private string? _token;

    public bool IsPopulated => _session != null;

    public void Populate(
        string token,
        SessionValidationResult session,
        ContractStatusInfo contract,
        ContractEntitlementsDto entitlements)
    {
        _token = token;
        _session = session;
        _contract = contract;
        _entitlements = entitlements;
    }

    public int UserId => _session?.UserId ?? throw NotPopulated();
    public int OrgId => _session?.OrgId ?? throw NotPopulated();
    public int ContractId => _session?.ContractId ?? throw NotPopulated();
    public string UserEmail => _session?.Email ?? throw NotPopulated();
    public string UserName => _session?.UserName ?? throw NotPopulated();
    public ContractStatus ContractStatus => _contract?.Status ?? throw NotPopulated();
    public DateOnly? ContractExpiry => _contract?.ExpiryDate;
    public bool IsContractActive => _contract?.IsActive ?? throw NotPopulated();
    public ContractEntitlementsDto Entitlements => _entitlements ?? throw NotPopulated();
    public bool HasEntitlement(EntitlementFeature feature) => Entitlements.IsFeatureEnabled(feature);
    public int DataSourceId => _session?.DataSourceId ?? throw NotPopulated();
    public string SessionToken => _token ?? throw NotPopulated();
    public DateTime SessionValidatedAt => _session?.ValidatedAt ?? throw NotPopulated();

    private static InvalidOperationException NotPopulated() =>
        new("IRequestContext has not been populated. Ensure RequestContextMiddleware runs before the controller.");
}
