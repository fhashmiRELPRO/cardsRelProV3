namespace RelPro.Infrastructure.Logging;

public sealed class UserQuotaRecord
{
    public int Id { get; set; }
    public int GrossQuotaUsage { get; set; }
    public int GrossQuotaLimit { get; set; }
    public int NetQuotaUsage { get; set; }
    public int NetQuotaLimit { get; set; }
    public DateTime? StartUsageTime { get; set; }
    public int PeriodId { get; set; }
    public int MethodId { get; set; }
}
