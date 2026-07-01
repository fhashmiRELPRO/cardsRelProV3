namespace RelPro.CARDS.Api.Auth.Options;

public sealed class ContractOptions
{
    public int MaxFreeTrial { get; set; } = 1000;
    public int FailureLimit { get; set; } = 10;
    public int FailureTimeMinutes { get; set; } = 30;
}
