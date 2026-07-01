using System.Data;
using Dapper;
using RelPro.Infrastructure.Database;

namespace RelPro.Infrastructure.Logging;

public sealed class DbRequestAuditLogger : IRequestAuditLogger
{
    private readonly IMySqlConnectionFactory _db;

    public DbRequestAuditLogger(IMySqlConnectionFactory db) => _db = db;

    public async Task LogAsync(AuditLogEntry e, CancellationToken ct = default)
    {
        await using var conn = await _db.OpenAsync(ct);

        var p = new DynamicParameters();
        p.Add("p_Id",                dbType: DbType.Int32, direction: ParameterDirection.Output);
        p.Add("p_user_id",           e.UserId);
        p.Add("p_method",            e.Method);
        p.Add("p_object_type",       e.ObjectType);
        p.Add("p_verb",              e.Verb);
        p.Add("p_state",             e.State);
        p.Add("p_parameters",        e.Parameters);
        p.Add("p_response_size",     e.ResponseSize);
        p.Add("p_error_message",     e.ErrorMessage);
        p.Add("p_duration",          e.Duration);
        p.Add("p_start_time",        e.StartTime);
        p.Add("p_party_type_id",     e.PartyTypeId);
        p.Add("p_data_source_id",    e.DataSourceId);
        p.Add("p_reference_id",      e.ReferenceId);
        p.Add("p_object_id",         e.ObjectId);
        p.Add("p_change_log",        e.ChangeLog);
        p.Add("p_method_id",         e.MethodId);
        p.Add("p_end_time",          e.EndTime);
        p.Add("p_logged",            e.Logged);
        p.Add("p_query_string",      e.QueryString);
        p.Add("p_caching",           e.Caching);
        p.Add("p_user_email",        e.UserEmail);
        p.Add("p_user_nonce",        e.UserNonce);
        p.Add("p_user_IP",           e.UserIp);
        p.Add("p_user_Port",         e.UserPort);
        p.Add("p_related_log_id",    e.RelatedLogId);
        p.Add("p_number_of_results", e.NumberOfResults);

        await conn.ExecuteAsync(
            "rpro3_create_log_3_3_0_4",
            p,
            commandType: CommandType.StoredProcedure);
    }
}
