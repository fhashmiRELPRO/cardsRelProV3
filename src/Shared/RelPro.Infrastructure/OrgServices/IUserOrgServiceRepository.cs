namespace RelPro.Infrastructure.OrgServices;

public interface IUserOrgServiceRepository
{
    Task<UserOrgServiceRecord?> GetAsync(int orgId, int dataSourceId, CancellationToken ct = default);
}
