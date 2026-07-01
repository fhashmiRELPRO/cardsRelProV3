using RelPro.Contracts.Dtos;
using RelPro.Infrastructure.Entitlements;

namespace RelPro.Search.IntegrationTests.Stubs;

public sealed class StubEntitlementLoader : IEntitlementLoader
{
    public Task<ContractEntitlementsDto> LoadAsync(int contractId, CancellationToken ct = default)
    {
        var instance = new ContractEntitlementsDto { ContractId = contractId };
        foreach (var prop in typeof(ContractEntitlementsDto)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(bool) && p.CanWrite))
        {
            prop.SetValue(instance, true);
        }
        return Task.FromResult(instance);
    }
}
