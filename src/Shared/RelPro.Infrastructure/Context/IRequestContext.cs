using RelPro.Contracts.Dtos;
using RelPro.Contracts.Enums;

namespace RelPro.Infrastructure.Context;

public interface IRequestContext
{
    bool IsPopulated { get; }
    int UserId { get; }
    int OrgId { get; }
    int ContractId { get; }
    string UserEmail { get; }
    string UserName { get; }
    ContractStatus ContractStatus { get; }
    DateOnly? ContractExpiry { get; }
    bool IsContractActive { get; }
    ContractEntitlementsDto Entitlements { get; }
    bool HasEntitlement(EntitlementFeature feature);
    int DataSourceId { get; }
    string SessionToken { get; }
    DateTime SessionValidatedAt { get; }
}
