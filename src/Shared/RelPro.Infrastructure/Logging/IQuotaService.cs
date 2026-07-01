namespace RelPro.Infrastructure.Logging;

public interface IQuotaService
{
    /// <summary>
    /// Returns true if the user has remaining quota; false if any active quota is exhausted.
    /// Automatically resets counters for quotas whose period has expired.
    /// </summary>
    Task<bool> CheckAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Increments gross_quota_usage by 1 for all active quota records belonging to the user.
    /// Call only after a successful response (HTTP < 400).
    /// </summary>
    Task IncrementAsync(int userId, CancellationToken ct = default);
}
