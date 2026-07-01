using Dapper;
using RelPro.Infrastructure.Database;

namespace RelPro.Infrastructure.Session;

public sealed class MySqlLegacySessionDataSource : ILegacySessionDataSource
{
    private readonly IMySqlConnectionFactory _db;

    // Query 1: session + user data only (no contract_id - not a column on users)
    private const string SessionSql = """
        SELECT
            s.user_id           AS UserId,
            u.organization_id   AS OrgId,
            u.email_address     AS Email,
            u.user_name         AS UserName
        FROM   sessions s
        JOIN   users u ON u.id = s.user_id
        WHERE  s.session_id = @Token
          AND  s.last_access_time > DATE_SUB(NOW(), INTERVAL @TimeoutMinutes MINUTE)
          AND  u.active  = 1
          AND  u.enabled = 1
        """;

    // Query 2: contract lookup - contract_users links users to contracts via user_id FK
    private const string ContractSql = """
        SELECT MAX(contract_id)
        FROM   contract_users
        WHERE  user_id      = @UserId
          AND  deleted_flag = 0
        """;

    public MySqlLegacySessionDataSource(IMySqlConnectionFactory db) => _db = db;

    public async Task<LegacySessionRow?> FindByTokenAsync(string token, int timeoutMinutes, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);

        var row = await conn.QuerySingleOrDefaultAsync<SessionUserRow>(SessionSql, new { Token = token, TimeoutMinutes = timeoutMinutes });
        if (row is null) return null;

        var contractId = await conn.ExecuteScalarAsync<int?>(ContractSql, new { UserId = row.UserId }) ?? 0;
        return new LegacySessionRow(row.UserId, row.OrgId, contractId, row.Email, row.UserName);
    }

    private sealed record SessionUserRow(int UserId, int OrgId, string Email, string UserName);
}
