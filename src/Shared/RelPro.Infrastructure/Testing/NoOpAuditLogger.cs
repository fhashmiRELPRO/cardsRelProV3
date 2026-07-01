using RelPro.Infrastructure.Logging;

namespace RelPro.Infrastructure.Testing;

/// <summary>Discards all audit log writes - use in integration tests to avoid a live MySQL connection.</summary>
public sealed class NoOpAuditLogger : IRequestAuditLogger
{
    public Task LogAsync(AuditLogEntry entry, CancellationToken ct = default) => Task.CompletedTask;
}
