using RelPro.CARDS.Api.Auth.Models;

namespace RelPro.CARDS.Api.Auth.Services;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default);
}
