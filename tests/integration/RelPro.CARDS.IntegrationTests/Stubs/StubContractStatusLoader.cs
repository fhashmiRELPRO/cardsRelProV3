using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Entitlements;

namespace RelPro.CARDS.IntegrationTests.Stubs;

public sealed class StubContractStatusLoader : IContractStatusLoader
{
    public Task<ContractStatusInfo?> LoadAsync(int contractId, CancellationToken ct = default)
    {
        if (contractId == 502)
            return Task.FromResult<ContractStatusInfo?>(
                new ContractStatusInfo(502, ContractStatus.Suspended, null, IsActive: false));

        return Task.FromResult<ContractStatusInfo?>(
            new ContractStatusInfo(contractId, ContractStatus.Active, new DateOnly(2030, 1, 1), IsActive: true));
    }
}
