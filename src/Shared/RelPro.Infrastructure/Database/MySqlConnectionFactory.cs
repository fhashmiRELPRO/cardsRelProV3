using Microsoft.Extensions.Options;
using MySqlConnector;

namespace RelPro.Infrastructure.Database;

public sealed class MySqlConnectionFactory : IMySqlConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IOptions<MySqlOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task<MySqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}

public sealed class MySqlOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}
