namespace RelPro.Infrastructure.OrgServices;

public sealed record UserOrgServiceRecord(
    int     Id,
    int     OrganizationId,
    int     DataSourceId,
    bool    AllowPeopleSearch,
    bool    AllowOrgSearch,
    string? SearchEndpoint,
    string? ProfileEndpoint,
    string? BaseUrl,
    string? AccessCode,
    string? Root,
    string? UserName,
    string? Password,
    string? AccessToken
);
