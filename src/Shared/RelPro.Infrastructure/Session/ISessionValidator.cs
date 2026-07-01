namespace RelPro.Infrastructure.Session;

public interface ISessionValidator
{
    Task<SessionValidationResult?> ValidateAsync(string token, CancellationToken ct = default);
}
