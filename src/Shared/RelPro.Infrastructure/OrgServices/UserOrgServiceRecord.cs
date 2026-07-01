namespace RelPro.Infrastructure.OrgServices;

/// <summary>
/// Maps one row of user_org_services (loaded via rcp2_read_org_services stored proc).
/// Root = MongoDB database name; ProfileEndpoint = MongoDB collection name for individual search.
/// </summary>
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
