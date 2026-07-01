using RelPro.Common.Exceptions;
using RelPro.CARDS.Api.User.Models;
using RelPro.CARDS.Api.User.Repositories;

namespace RelPro.CARDS.Api.User.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo) => _repo = repo;

    public async Task<UserDetailResponse> GetByIdAsync(
        int requestingOrgId, int userId, CancellationToken ct = default)
    {
        var row = await _repo.GetByIdAsync(userId, ct)
            ?? throw new ResourceNotFoundException("User", userId);

        // Callers may only look up users within their own org - don't reveal cross-org existence
        if (row.OrganizationId != requestingOrgId)
            throw new ResourceNotFoundException("User", userId);

        return new UserDetailResponse(
            UserId:         row.Id,
            Name:           row.Name,
            Email:          row.Email,
            OrgId:          row.OrganizationId,
            OrgName:        row.OrganizationName,
            FirstName:      row.FirstName,
            LastName:       row.LastName,
            Headline:       row.Headline,
            IsActive:       row.Active,
            IsEnabled:      row.Enabled,
            IsAdmin:        row.IsAdmin,
            LicenseExpires: row.DateLicenseExpires
        );
    }
}
