using RelPro.Infrastructure.Logging;

namespace RelPro.Infrastructure.Testing;

public sealed class NoOpQuotaService : IQuotaService
{
    public Task<bool> CheckAsync(int userId, CancellationToken ct = default) => Task.FromResult(true);
    public Task IncrementAsync(int userId, CancellationToken ct = default) => Task.CompletedTask;
}
