using RelPro.CARDS.Api.Search.Models;
using RelPro.CARDS.Api.Search.Repositories;

namespace RelPro.CARDS.IntegrationTests.Stubs;

public sealed class StubPersonSearchRepository : IPersonSearchRepository
{
    public Task<IReadOnlyList<PersonResult>> SearchAsync(string query, int limit, CancellationToken ct = default)
    {
        if (query == "simulate-error")
            throw new InvalidOperationException("Simulated database failure.");

        var results = Enumerable.Range(1, Math.Min(limit, 3))
            .Select(i => new PersonResult(
                Id: i,
                FirstName: $"Person{i}",
                LastName: "Test",
                MiddleName: null,
                Organization: "Test Bank",
                Title: "Manager"))
            .ToList();

        return Task.FromResult<IReadOnlyList<PersonResult>>(results);
    }
}
