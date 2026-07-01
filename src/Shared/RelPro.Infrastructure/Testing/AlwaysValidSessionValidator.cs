using RelPro.Infrastructure.Session;

namespace RelPro.Infrastructure.Testing;

public sealed class AlwaysValidSessionValidator : ISessionValidator
{
    private readonly SessionValidationResult _result;

    public AlwaysValidSessionValidator(SessionValidationResult? result = null)
    {
        _result = result ?? new SessionValidationResult(
            UserId: 1001,
            OrgId: 10,
            ContractId: 500,
            Email: "test@relpro.com",
            UserName: "Test User",
            DataSourceId: 1,
            ValidatedAt: DateTime.UtcNow);
    }

    public Task<SessionValidationResult?> ValidateAsync(string token, CancellationToken ct = default) =>
        Task.FromResult<SessionValidationResult?>(_result);
}

public sealed class AlwaysInvalidSessionValidator : ISessionValidator
{
    public Task<SessionValidationResult?> ValidateAsync(string token, CancellationToken ct = default) =>
        Task.FromResult<SessionValidationResult?>(null);
}
