using RelPro.CARDS.Api.User.Models;

namespace RelPro.CARDS.Api.User.Services;

public interface IUserService
{
    Task<UserDetailResponse> GetByIdAsync(int requestingOrgId, int userId, CancellationToken ct = default);
}
