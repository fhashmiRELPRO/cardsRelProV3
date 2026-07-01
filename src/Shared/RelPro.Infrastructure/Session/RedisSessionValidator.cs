namespace RelPro.Infrastructure.Session;

public sealed class RedisSessionValidator : ISessionValidator
{
    private readonly ISessionStore _store;

    public RedisSessionValidator(ISessionStore store) => _store = store;

    public async Task<SessionValidationResult?> ValidateAsync(string token, CancellationToken ct = default)
    {
        var data = await _store.GetAsync(token, ct);
        if (data is null) return null;

        return new SessionValidationResult(
            data.UserId, data.OrgId, data.ContractId,
            data.Email, data.UserName, data.DataSourceId, data.ValidatedAt);
    }
}
