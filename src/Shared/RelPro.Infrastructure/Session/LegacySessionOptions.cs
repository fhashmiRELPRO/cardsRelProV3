namespace RelPro.Infrastructure.Session;

public sealed class LegacySessionOptions
{
    public int SessionTimeoutMinutes { get; init; } = 720;
    public int CacheMinutes { get; init; } = 5;
}
