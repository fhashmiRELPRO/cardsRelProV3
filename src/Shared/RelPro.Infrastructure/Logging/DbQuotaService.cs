using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using RelPro.Infrastructure.Database;

namespace RelPro.Infrastructure.Logging;

public sealed class DbQuotaService : IQuotaService
{
    private readonly IMySqlConnectionFactory _db;
    private readonly ILogger<DbQuotaService> _logger;

    public DbQuotaService(IMySqlConnectionFactory db, ILogger<DbQuotaService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> CheckAsync(int userId, CancellationToken ct = default)
    {
        var quotas = await LoadAsync(userId, ct);

        foreach (var q in quotas)
        {
            if (IsPeriodExpired(q))
            {
                await UpdateUsageAsync(q.Id, netUsage: 0, grossUsage: 0, ct);
                continue;
            }

            if (q.GrossQuotaLimit > 0 && q.GrossQuotaUsage >= q.GrossQuotaLimit)
            {
                _logger.LogWarning(
                    "Quota exceeded for user {UserId}: quota {QuotaId} usage {Usage}/{Limit}",
                    userId, q.Id, q.GrossQuotaUsage, q.GrossQuotaLimit);
                return false;
            }
        }

        return true;
    }

    public async Task IncrementAsync(int userId, CancellationToken ct = default)
    {
        var quotas = await LoadAsync(userId, ct);

        foreach (var q in quotas)
        {
            if (IsPeriodExpired(q)) continue;
            await UpdateUsageAsync(q.Id, q.NetQuotaUsage, q.GrossQuotaUsage + 1, ct);
        }
    }

    private async Task<List<UserQuotaRecord>> LoadAsync(int userId, CancellationToken ct)
    {
        await using var conn = await _db.OpenAsync(ct);
        var rows = await conn.QueryAsync<UserQuotaRecord>(
            @"SELECT uq.id                 AS Id,
                     uq.gross_quota_usage  AS GrossQuotaUsage,
                     uq.gross_quota_limit  AS GrossQuotaLimit,
                     uq.net_quota_usage    AS NetQuotaUsage,
                     uq.net_quota_limit    AS NetQuotaLimit,
                     uq.start_usage_time   AS StartUsageTime,
                     q.period_id           AS PeriodId,
                     q.method_id           AS MethodId
              FROM   user_quotas uq
              JOIN   quotas      q  ON q.id = uq.quota_id
              WHERE  uq.user_id     = @userId
                AND  uq.deleted_flag = 0",
            new { userId });

        return rows.ToList();
    }

    private async Task UpdateUsageAsync(int id, int netUsage, int grossUsage, CancellationToken ct)
    {
        await using var conn = await _db.OpenAsync(ct);
        await conn.ExecuteAsync(
            "rpro2_update_user_quota_2_4_4_6",
            new { p_Id = id, p_net_quota_usage = netUsage, p_gross_quota_usage = grossUsage },
            commandType: CommandType.StoredProcedure);
    }

    // Period IDs mirror the legacy quotas.period_id column:
    // 1=one_time, 2=hourly, 3=daily, 4=weekly, 5=monthly, 6=quarterly, 7=annually
    private static bool IsPeriodExpired(UserQuotaRecord q)
    {
        if (q.StartUsageTime is null) return false;
        var start = q.StartUsageTime.Value;
        var now = DateTime.UtcNow;
        return q.PeriodId switch
        {
            2 => now >= start.AddHours(1),
            3 => now >= start.AddDays(1),
            4 => now >= start.AddDays(7),
            5 => now >= start.AddMonths(1),
            6 => now >= start.AddMonths(3),
            7 => now >= start.AddYears(1),
            _ => false
        };
    }
}
