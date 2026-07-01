using RelPro.Contracts.Dtos;
using RelPro.Infrastructure.Entitlements;

namespace RelPro.Infrastructure.Session;

public sealed class RedisEntitlementLoader : IEntitlementLoader
{
    private readonly ISessionStore _store;
    private readonly IHttpContextTokenExtractor _tokenExtractor;

    public RedisEntitlementLoader(ISessionStore store, IHttpContextTokenExtractor tokenExtractor)
    {
        _store = store;
        _tokenExtractor = tokenExtractor;
    }

    public async Task<ContractEntitlementsDto> LoadAsync(int contractId, CancellationToken ct = default)
    {
        var token = _tokenExtractor.CurrentToken;
        if (token is not null)
        {
            var data = await _store.GetAsync(token, ct);
            if (data?.ContractId == contractId)
                return data.Entitlements;
        }

        return new ContractEntitlementsDto { ContractId = contractId };
    }
}
