namespace RelPro.Infrastructure.Session;

public sealed record LegacySessionRow(
    int UserId,
    int OrgId,
    int ContractId,
    string Email,
    string UserName);

public interface ILegacySessionDataSource
{
    Task<LegacySessionRow?> FindByTokenAsync(string token, int timeoutMinutes, CancellationToken ct = default);
}
