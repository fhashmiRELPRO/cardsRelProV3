using Dapper;
using MySqlConnector;
using RelPro.Contracts.Common;
using System.Data;

namespace RelPro.Infrastructure.Database;

/// <summary>
/// Abstract base for all MySQL repositories. Subclasses inject IMySqlConnectionFactory
/// and call the protected helpers - no raw Dapper boilerplate in domain repositories.
/// </summary>
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

    /// <summary>
    /// Runs a count query and a paged data query in the same open connection.
    /// countSql must return a single integer. dataSql must include LIMIT and OFFSET.
    /// </summary>
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

    /// <summary>
    /// Runs multiple operations inside a single MySQL transaction.
    /// The work delegate receives an open connection + active transaction.
    /// </summary>
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

    /// <summary>
    /// Calls a MySQL stored procedure. Use this to invoke existing CLIDE procedures
    /// without rewriting them. The procedure name is passed as-is to Dapper.
    /// </summary>
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

    /// <summary>
    /// Calls a stored procedure that returns a single row or null.
    /// </summary>
    protected async Task<T?> CallProcedureSingleAsync<T>(
        string procedureName, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        return await conn.QuerySingleOrDefaultAsync<T>(new CommandDefinition(
            procedureName, param,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    /// <summary>
    /// Calls a stored procedure that performs an INSERT/UPDATE/DELETE (no result set).
    /// </summary>
    protected async Task<int> ExecuteProcedureAsync(
        string procedureName, object? param = null, CancellationToken ct = default)
    {
        using var conn = await _db.OpenAsync(ct);
        return await conn.ExecuteAsync(new CommandDefinition(
            procedureName, param,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    /// <summary>
    /// Transaction variant that returns a value.
    /// </summary>
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
