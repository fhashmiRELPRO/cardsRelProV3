using RelPro.Infrastructure.Email;

namespace RelPro.Infrastructure.Testing;

public sealed class NoOpEmailService : IEmailService
{
    public Task SendQuotaExceededAsync(string userEmail, string userName, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SendLoginFailureAlertAsync(string accountEmail, CancellationToken ct = default)
        => Task.CompletedTask;
}
