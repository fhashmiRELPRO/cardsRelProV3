namespace RelPro.Infrastructure.Email;

public sealed class EmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Ssl { get; set; } = "TLS/SSL, RFC3207";
    public string FromAddress { get; set; } = string.Empty;
    public string InternalTo { get; set; } = string.Empty;

    // Quota-exceeded notification (all services with RequestLoggingMiddleware)
    public string UserQuotaEmailSubject { get; set; } = "Your RelPro License Needs to Be Renewed/Upgraded";
    public string UserQuotaEmailTemplate { get; set; } = string.Empty;
    public string UserQuotaAdminSubject { get; set; } = "{0}'s RelPro License Has Ended";
    public string UserQuotaAdminTemplate { get; set; } = string.Empty;

    // Login failure alert (Auth.Service only)
    public string LoginFailureNotify { get; set; } = string.Empty;
    public int LoginFailureLimit { get; set; } = 5;
}
