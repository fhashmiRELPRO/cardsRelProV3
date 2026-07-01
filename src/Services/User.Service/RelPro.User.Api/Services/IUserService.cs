using RelPro.User.Api.Models;

namespace RelPro.User.Api.Services;

public interface IUserService
{
    Task<UserDetailResponse> GetByIdAsync(int requestingOrgId, int userId, CancellationToken ct = default);
}
