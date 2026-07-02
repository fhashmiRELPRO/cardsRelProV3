namespace RelPro.Infrastructure.Logging;

public sealed class AuditLogEntry
{
    public int UserId { get; init; }
    public string Method { get; init; } = string.Empty;
    public string ObjectType { get; init; } = string.Empty;
    public string Verb { get; init; } = string.Empty;
    public int State { get; init; }
    public string? Parameters { get; init; }
    public int ResponseSize { get; init; }
    public string? ErrorMessage { get; init; }
    public float Duration { get; init; }
    public DateTime StartTime { get; init; }
    public int PartyTypeId { get; init; }
    public int DataSourceId { get; init; }
    public string? ReferenceId { get; init; }
    public int ObjectId { get; init; }
    public string? ChangeLog { get; init; }
    public int MethodId { get; init; }
    public DateTime EndTime { get; init; }
    public byte Logged { get; init; } = 1;
    public string? QueryString { get; init; }
    public int Caching { get; init; }
    public string? UserEmail { get; init; }
    public string? UserNonce { get; init; }
    public string? UserIp { get; init; }
    public int UserPort { get; init; }
    public int RelatedLogId { get; init; }
    public int NumberOfResults { get; init; }
}
