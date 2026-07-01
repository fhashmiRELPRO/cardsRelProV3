using RelPro.Infrastructure.Session;

namespace RelPro.CARDS.IntegrationTests.Stubs;

/// <summary>
/// Maps well-known token strings to specific sessions for integration testing.
/// Anything else → null (invalid session → 401).
/// </summary>
public sealed class TokenAwareSessionValidator : ISessionValidator
{
    // ContractId 500 = active, all entitlements
    // ContractId 501 = active, no entitlements
    // ContractId 502 = inactive contract
    public const string ValidToken            = "valid-user-token";
    public const string NoEntitlementToken    = "no-entitlement-token";
    public const string InactiveContractToken = "inactive-contract-token";

    public Task<SessionValidationResult?> ValidateAsync(string token, CancellationToken ct = default)
    {
        SessionValidationResult? result = token switch
        {
            ValidToken => new SessionValidationResult(
                UserId: 1001, OrgId: 10, ContractId: 500,
                Email: "test@relpro.com", UserName: "Test User",
                DataSourceId: 1, ValidatedAt: DateTime.UtcNow),

            NoEntitlementToken => new SessionValidationResult(
                UserId: 1002, OrgId: 10, ContractId: 501,
                Email: "noaccess@relpro.com", UserName: "No Access User",
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
