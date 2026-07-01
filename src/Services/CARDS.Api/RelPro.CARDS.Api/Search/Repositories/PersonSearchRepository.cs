using Dapper;
using RelPro.Infrastructure.Database;
using RelPro.CARDS.Api.Search.Models;

namespace RelPro.CARDS.Api.Search.Repositories;

public sealed class PersonSearchRepository : IPersonSearchRepository
{
    private readonly IMySqlConnectionFactory _db;

    public PersonSearchRepository(IMySqlConnectionFactory db) => _db = db;

    public async Task<IReadOnlyList<PersonResult>> SearchAsync(string query, int limit, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        var like = $"%{query.Trim()}%";
        var rows = await conn.QueryAsync<PersonResult>(
            @"SELECT id            AS Id,
                     first_name    AS FirstName,
                     last_name     AS LastName,
                     middle_name   AS MiddleName,
                     organization_name AS Organization,
                     title         AS Title
              FROM   individuals
              WHERE  first_name LIKE @like
                  OR last_name  LIKE @like
                  OR CONCAT(first_name, ' ', last_name) LIKE @like
              ORDER BY last_name, first_name
              LIMIT  @limit",
            new { like, limit });

        return rows.ToList();
    }
}
