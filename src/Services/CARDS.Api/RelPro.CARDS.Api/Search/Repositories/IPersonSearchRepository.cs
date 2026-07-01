using RelPro.CARDS.Api.Search.Models;

namespace RelPro.CARDS.Api.Search.Repositories;

public interface IPersonSearchRepository
{
    Task<IReadOnlyList<PersonResult>> SearchAsync(string query, int limit, CancellationToken ct = default);
}
