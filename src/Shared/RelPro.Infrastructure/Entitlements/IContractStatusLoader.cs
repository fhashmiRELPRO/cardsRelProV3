namespace RelPro.Infrastructure.Entitlements;

public interface IContractStatusLoader
{
    Task<ContractStatusInfo?> LoadAsync(int contractId, CancellationToken ct = default);
}
