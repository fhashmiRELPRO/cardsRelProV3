using RelPro.Infrastructure.Entitlements;

namespace RelPro.Infrastructure.Session;

public sealed class RedisContractStatusLoader : IContractStatusLoader
{
    private readonly ISessionStore _store;
    private readonly IHttpContextTokenExtractor _tokenExtractor;

    public RedisContractStatusLoader(ISessionStore store, IHttpContextTokenExtractor tokenExtractor)
    {
        _store = store;
        _tokenExtractor = tokenExtractor;
    }

    public async Task<ContractStatusInfo?> LoadAsync(int contractId, CancellationToken ct = default)
    {
        var token = _tokenExtractor.CurrentToken;
        if (token is null) return null;

        var data = await _store.GetAsync(token, ct);
        if (data is null || data.ContractId != contractId) return null;

        return new ContractStatusInfo(data.ContractId, data.ContractStatus, data.ContractExpiry, data.IsContractActive);
    }
}
