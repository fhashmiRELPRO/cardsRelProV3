using Dapper;
using RelPro.CARDS.Api.Auth.Models;
using RelPro.Infrastructure.Database;

namespace RelPro.CARDS.Api.Auth.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IMySqlConnectionFactory _db;

    public UserRepository(IMySqlConnectionFactory db) => _db = db;

    public async Task<UserRecord?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "rpro2_find_user",
            new { p_email = email },
            commandType: System.Data.CommandType.StoredProcedure);

        if (row is null) return null;

        return new UserRecord(
            Id: (int)row.id,
            OrgId: (int)(row.organizationId ?? row.organization_id ?? 0),
            ContractId: (int)(row.contractId ?? row.contract_id ?? 0),
            Email: (string)row.email,
            FirstName: (string)(row.firstName ?? row.first_name ?? ""),
            LastName: (string)(row.lastName ?? row.last_name ?? ""),
            UserName: (string?)(row.user_name ?? row.username),
            PasswordHash: (string)(row.password ?? ""),
            Salt: (string?)(row.salt),
            IsActive: (bool)(row.active ?? true),
            IsEnabled: (bool)(row.enabled ?? true));
    }
}
