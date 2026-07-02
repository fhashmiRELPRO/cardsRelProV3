using Dapper;
using MySqlConnector;
using RelPro.Contracts.Common;
using System.Data;

namespace RelPro.Infrastructure.Database;

public abstract class BaseMySqlRepository
{
    private readonly IMySqlConnectionFactory _db;

    protected BaseMySqlRepository(IMySqlConnectionFactory db) => _db = db;

    protected async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        var result = await conn.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
        return result.AsList();
    }

    protected async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    protected async Task<int> ExecuteAsync(
        string sql, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        return await conn.ExecuteAsync(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    protected async Task<T?> ExecuteScalarAsync<T>(
        string sql, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        return await conn.ExecuteScalarAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    protected async Task<PagedResult<T>> QueryPagedAsync<T>(
        string countSql, string dataSql, object? param = null,
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);

        var totalCount = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: ct));

        if (totalCount == 0)
            return PagedResult<T>.Empty(page, pageSize);

        var items = await conn.QueryAsync<T>(
            new CommandDefinition(dataSql, param, cancellationToken: ct));

        return new PagedResult<T>
        {
            Items = items.AsList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    protected async Task ExecuteTransactionAsync(
        Func<MySqlConnection, MySqlTransaction, Task> work,
        CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);
        try
        {
            await work(conn, tx);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    protected async Task<IReadOnlyList<T>> CallProcedureAsync<T>(
        string procedureName, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        var result = await conn.QueryAsync<T>(new CommandDefinition(
            procedureName, param,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
        return result.AsList();
    }

    protected async Task<T?> CallProcedureSingleAsync<T>(
        string procedureName, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<T>(new CommandDefinition(
            procedureName, param,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    protected async Task<int> ExecuteProcedureAsync(
        string procedureName, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        return await conn.ExecuteAsync(new CommandDefinition(
            procedureName, param,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    protected async Task<T> ExecuteTransactionAsync<T>(
        Func<MySqlConnection, MySqlTransaction, Task<T>> work,
        CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);
        try
        {
            var result = await work(conn, tx);
            await tx.CommitAsync(ct);
            return result;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
