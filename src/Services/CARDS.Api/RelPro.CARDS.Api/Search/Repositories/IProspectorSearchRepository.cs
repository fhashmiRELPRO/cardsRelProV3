using RelPro.CARDS.Api.Search.Models;

namespace RelPro.CARDS.Api.Search.Repositories;

public interface IProspectorSearchRepository
{
    Task<ProspectorSearchResponse> SearchAsync(ProspectorSearchRequest request, CancellationToken ct = default);
}
