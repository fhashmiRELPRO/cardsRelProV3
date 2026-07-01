namespace RelPro.Infrastructure.OrgServices;

public interface IUserOrgServiceRepository
{
    /// <summary>
    /// Returns the service config for the given org and data source, or null if the org
    /// has no record for that data source type (i.e. the feature is not provisioned).
    /// Results are Redis-cached per org for 30 minutes.
    /// </summary>
    Task<UserOrgServiceRecord?> GetAsync(int orgId, int dataSourceId, CancellationToken ct = default);
}
