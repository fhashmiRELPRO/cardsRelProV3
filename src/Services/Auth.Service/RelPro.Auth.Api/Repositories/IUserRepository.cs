using RelPro.Auth.Api.Models;

namespace RelPro.Auth.Api.Repositories;

public interface IUserRepository
{
    Task<UserRecord?> FindByEmailAsync(string email, CancellationToken ct = default);
}
