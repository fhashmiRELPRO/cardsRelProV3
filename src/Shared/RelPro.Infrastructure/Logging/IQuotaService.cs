namespace RelPro.Infrastructure.Logging;

public interface IQuotaService
{
    Task<bool> CheckAsync(int userId, CancellationToken ct = default);

    Task IncrementAsync(int userId, CancellationToken ct = default);
}
