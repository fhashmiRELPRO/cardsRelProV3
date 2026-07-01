using RelPro.Contracts.Dtos;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Entitlements;

namespace RelPro.Infrastructure.Session;

public sealed record FullSessionData(
    int UserId,
    int OrgId,
    int ContractId,
    string Email,
    string UserName,
    int DataSourceId,
    DateTime ValidatedAt,
    ContractStatus ContractStatus,
    DateOnly? ContractExpiry,
    bool IsContractActive,
    ContractEntitlementsDto Entitlements
);

public interface ISessionStore
{
    Task<string> CreateAsync(FullSessionData data, TimeSpan? ttl = null, CancellationToken ct = default);
    Task<FullSessionData?> GetAsync(string token, CancellationToken ct = default);
    Task InvalidateAsync(string token, CancellationToken ct = default);
}
