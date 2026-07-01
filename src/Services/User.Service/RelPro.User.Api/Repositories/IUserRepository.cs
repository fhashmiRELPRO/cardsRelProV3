namespace RelPro.User.Api.Repositories;

public interface IUserRepository
{
    Task<UserRow?> GetByIdAsync(int userId, CancellationToken ct = default);
}
