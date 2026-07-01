namespace RelPro.CARDS.Api.Auth.Services;

public interface ILoginLockoutService
{
    Task<bool> IsLockedAsync(string email, CancellationToken ct = default);
    Task RecordFailureAsync(string email, CancellationToken ct = default);
    Task ClearAsync(string email, CancellationToken ct = default);
}
