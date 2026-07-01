using RelPro.Infrastructure.Logging;

namespace RelPro.Infrastructure.Testing;

/// <summary>Always allows requests and never increments counters - use in integration tests to avoid a live MySQL connection.</summary>
public sealed class NoOpQuotaService : IQuotaService
{
    public Task<bool> CheckAsync(int userId, CancellationToken ct = default) => Task.FromResult(true);
    public Task IncrementAsync(int userId, CancellationToken ct = default) => Task.CompletedTask;
}
