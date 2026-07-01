using RelPro.CARDS.Api.Auth.Models;

namespace RelPro.CARDS.Api.Auth.Repositories;

public interface IUserRepository
{
    Task<UserRecord?> FindByEmailAsync(string email, CancellationToken ct = default);
}
