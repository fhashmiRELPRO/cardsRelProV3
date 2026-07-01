using RelPro.User.Api.Repositories;

namespace RelPro.User.IntegrationTests.Stubs;

/// <summary>
/// In-memory stub for IUserRepository.
/// userId=1001 → user in org 10 (matching test caller)
/// userId=1099 → user in org 99 (different org - triggers security 404)
/// userId=9999 → null (not found → 404)
/// userId=500  → throws to simulate an unexpected DB error (→ 500)
/// </summary>
public sealed class StubUserRepository : IUserRepository
{
    public Task<UserRow?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        if (userId == 500)
            throw new InvalidOperationException("Simulated database failure.");

        UserRow? row = userId switch
        {
            1001 => new UserRow
            {
                Id = 1001, OrganizationId = 10,
                Name = "Jane Smith", Email = "jane@relpro.com",
                FirstName = "Jane", LastName = "Smith",
                Headline = "Product Manager", Active = true, Enabled = true,
                IsAdmin = false, DateLicenseExpires = "2030-01-01",
                OrganizationName = "Test Bank"
            },
            1099 => new UserRow
            {
                Id = 1099, OrganizationId = 99,  // different org
                Name = "Other Org User", Email = "other@bank.com",
                Active = true, Enabled = true
            },
            _ => null
        };

        return Task.FromResult(row);
    }
}
