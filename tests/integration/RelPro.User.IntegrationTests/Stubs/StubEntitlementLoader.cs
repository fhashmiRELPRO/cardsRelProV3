using RelPro.Contracts.Dtos;
using RelPro.Infrastructure.Entitlements;

namespace RelPro.User.IntegrationTests.Stubs;

public sealed class StubEntitlementLoader : IEntitlementLoader
{
    public Task<ContractEntitlementsDto> LoadAsync(int contractId, CancellationToken ct = default)
    {
        // ContractId 501 = no entitlements; all others get everything enabled.
        var dto = contractId == 501
            ? new ContractEntitlementsDto { ContractId = contractId }
            : BuildAllEntitlements(contractId);

        return Task.FromResult(dto);
    }

    private static ContractEntitlementsDto BuildAllEntitlements(int contractId)
    {
        var instance = new ContractEntitlementsDto { ContractId = contractId };
        foreach (var prop in typeof(ContractEntitlementsDto)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(bool) && p.CanWrite))
        {
            prop.SetValue(instance, true);
        }
        return instance;
    }
}
