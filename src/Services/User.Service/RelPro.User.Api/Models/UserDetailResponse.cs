namespace RelPro.User.Api.Models;

public sealed record UserDetailResponse(
    int     UserId,
    string  Name,
    string  Email,
    int     OrgId,
    string? OrgName,
    string? FirstName,
    string? LastName,
    string? Headline,
    bool    IsActive,
    bool    IsEnabled,
    bool    IsAdmin,
    string? LicenseExpires,
    string? PendoAccountId = null
);
