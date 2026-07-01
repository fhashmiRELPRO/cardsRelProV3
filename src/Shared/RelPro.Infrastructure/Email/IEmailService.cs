namespace RelPro.Infrastructure.Email;

public interface IEmailService
{
    Task SendQuotaExceededAsync(string userEmail, string userName, CancellationToken ct = default);
    Task SendLoginFailureAlertAsync(string accountEmail, CancellationToken ct = default);
}
