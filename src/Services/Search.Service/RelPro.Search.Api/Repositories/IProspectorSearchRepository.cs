using RelPro.Search.Api.Models;

namespace RelPro.Search.Api.Repositories;

public interface IProspectorSearchRepository
{
    Task<ProspectorSearchResponse> SearchAsync(ProspectorSearchRequest request, CancellationToken ct = default);
}
