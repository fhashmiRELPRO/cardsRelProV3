using RelPro.Infrastructure.Database;

namespace RelPro.User.Api.Repositories;

public sealed class MySqlUserRepository : BaseMySqlRepository, IUserRepository
{
    public MySqlUserRepository(IMySqlConnectionFactory db) : base(db) { }

    public Task<UserRow?> GetByIdAsync(int userId, CancellationToken ct = default) =>
        CallProcedureSingleAsync<UserRow>("rcp2_read_user", new { p_Id = userId }, ct);
}
