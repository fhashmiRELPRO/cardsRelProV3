using RelPro.Infrastructure.Session;

namespace RelPro.Search.IntegrationTests.Stubs;

public sealed class TokenAwareSessionValidator : ISessionValidator
{
    public const string ValidToken            = "valid-user-token";
    public const string InactiveContractToken = "inactive-contract-token";

    public Task<SessionValidationResult?> ValidateAsync(string token, CancellationToken ct = default)
    {
        SessionValidationResult? result = token switch
        {
            ValidToken => new SessionValidationResult(
                UserId: 1001, OrgId: 10, ContractId: 500,
                Email: "test@relpro.com", UserName: "Test User",
                DataSourceId: 1, ValidatedAt: DateTime.UtcNow),

            InactiveContractToken => new SessionValidationResult(
                UserId: 1003, OrgId: 10, ContractId: 502,
                Email: "inactive@relpro.com", UserName: "Inactive User",
                DataSourceId: 1, ValidatedAt: DateTime.UtcNow),

            _ => null
        };

        return Task.FromResult(result);
    }
}
