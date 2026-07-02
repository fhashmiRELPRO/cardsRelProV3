using RelPro.Infrastructure.Logging;

namespace RelPro.Infrastructure.Testing;

public sealed class NoOpAuditLogger : IRequestAuditLogger
{
    public Task LogAsync(AuditLogEntry entry, CancellationToken ct = default) => Task.CompletedTask;
}
