using MySqlConnector;

namespace RelPro.Infrastructure.Database;

public interface IMySqlConnectionFactory
{
    Task<MySqlConnection> OpenAsync(CancellationToken ct = default);
}
