using RelPro.Contracts.Dtos;

namespace RelPro.Infrastructure.Entitlements;

public interface IEntitlementLoader
{
    Task<ContractEntitlementsDto> LoadAsync(int contractId, CancellationToken ct = default);
}
