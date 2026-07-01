using RelPro.Auth.Api.Models;

namespace RelPro.Auth.Api.Services;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default);
}
