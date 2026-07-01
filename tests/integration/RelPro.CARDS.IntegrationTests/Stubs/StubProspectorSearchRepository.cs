using RelPro.CARDS.Api.Search.Models;
using RelPro.CARDS.Api.Search.Repositories;

namespace RelPro.CARDS.IntegrationTests.Stubs;

public sealed class StubProspectorSearchRepository : IProspectorSearchRepository
{
    public Task<ProspectorSearchResponse> SearchAsync(ProspectorSearchRequest request, CancellationToken ct = default)
    {
        if (request.Query == "simulate-timeout")
            throw new TimeoutException("Simulated Atlas Search timeout.");

        var individuals = Enumerable.Range(1, Math.Min(request.SafePageSize, 3))
            .Select(i => new IndividualResult { RcpId = i, FirstName = $"Person{i}", LastName = "Test" })
            .ToList();

        var response = new ProspectorSearchResponse
        {
            TotalIndividuals = 42,
            Individuals = individuals,
            SearchId = 100
        };

        return Task.FromResult(response);
    }
}
