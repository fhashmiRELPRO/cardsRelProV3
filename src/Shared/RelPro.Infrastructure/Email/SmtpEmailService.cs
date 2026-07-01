using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RelPro.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _opts;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailOptions> opts, ILogger<SmtpEmailService> logger)
    {
        _opts = opts.Value;
        _logger = logger;
    }

    public async Task SendQuotaExceededAsync(string userEmail, string userName, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_opts.Host) || string.IsNullOrEmpty(userEmail))
            return;

        await SendAsync(userEmail, _opts.UserQuotaEmailSubject,
            BuildQuotaUserBody(userName), ct);

        if (!string.IsNullOrEmpty(_opts.InternalTo))
        {
            var adminSubject = string.Format(_opts.UserQuotaAdminSubject, userName);
            await SendAsync(_opts.InternalTo, adminSubject,
                BuildQuotaAdminBody(userName, userEmail), ct);
        }
    }

    public async Task SendLoginFailureAlertAsync(string accountEmail, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_opts.Host) || string.IsNullOrEmpty(_opts.LoginFailureNotify))
            return;

        await SendAsync(_opts.LoginFailureNotify,
            $"Login Failure Alert: {accountEmail}",
            BuildLoginFailureBody(accountEmail), ct);
    }

    private async Task SendAsync(string to, string subject, string body, CancellationToken ct)
    {
        try
        {
            using var client = new SmtpClient(_opts.Host, _opts.Port)
            {
                Credentials = new NetworkCredential(_opts.User, _opts.Password),
                EnableSsl = !string.IsNullOrEmpty(_opts.Ssl) &&
                            !_opts.Ssl.Equals("None", StringComparison.OrdinalIgnoreCase)
            };

            var from = !string.IsNullOrEmpty(_opts.FromAddress) ? _opts.FromAddress : _opts.User;
            using var message = new MailMessage(from, to, subject, body) { IsBodyHtml = true };

            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email send failed to {To} with subject {Subject}", to, subject);
        }
    }

    private static string BuildQuotaUserBody(string userName) =>
        $"""
        <html><body style="font-family:sans-serif;color:#333;padding:20px">
        <h2>RelPro License Renewal Required</h2>
        <p>Dear {WebUtility.HtmlEncode(userName)},</p>
        <p>Your RelPro account has reached its usage limit for this period.</p>
        <p>Please contact your administrator to renew or upgrade your license to continue accessing RelPro.</p>
        <p>Thank you,<br/>The RelPro Team</p>
        </body></html>
        """;

    private static string BuildQuotaAdminBody(string userName, string userEmail) =>
        $"""
        <html><body style="font-family:sans-serif;color:#333;padding:20px">
        <h2>User Quota Limit Reached</h2>
        <p><strong>{WebUtility.HtmlEncode(userName)}</strong> ({WebUtility.HtmlEncode(userEmail)})
        has exhausted their RelPro usage quota for this period.</p>
        <p>Please review their license or contact them to arrange a renewal or upgrade.</p>
        </body></html>
        """;

    private static string BuildLoginFailureBody(string accountEmail) =>
        $"""
        <html><body style="font-family:sans-serif;color:#333;padding:20px">
        <h2>Login Failure Alert</h2>
        <p>Multiple failed login attempts detected for account:
        <strong>{WebUtility.HtmlEncode(accountEmail)}</strong></p>
        <p>The account has been temporarily locked. Please investigate if suspicious activity is suspected.</p>
        </body></html>
        """;
}
