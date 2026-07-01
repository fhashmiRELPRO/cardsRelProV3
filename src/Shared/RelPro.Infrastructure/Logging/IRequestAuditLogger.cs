namespace RelPro.Infrastructure.Logging;

public interface IRequestAuditLogger
{
    Task LogAsync(AuditLogEntry entry, CancellationToken ct = default);
}
