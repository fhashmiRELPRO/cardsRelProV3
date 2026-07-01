using RelPro.Search.Api.Models;

namespace RelPro.Search.Api.Repositories;

public interface IPersonSearchRepository
{
    Task<IReadOnlyList<PersonResult>> SearchAsync(string query, int limit, CancellationToken ct = default);
}
