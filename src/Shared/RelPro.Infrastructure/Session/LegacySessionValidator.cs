using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace RelPro.Infrastructure.Session;

public sealed class LegacySessionValidator : ISessionValidator
{
    private readonly ILegacySessionDataSource _dataSource;
    private readonly IDistributedCache _cache;
    private readonly LegacySessionOptions _options;
    private readonly ILogger<LegacySessionValidator> _logger;

    public LegacySessionValidator(
        ILegacySessionDataSource dataSource,
        IDistributedCache cache,
        IOptions<LegacySessionOptions> options,
        ILogger<LegacySessionValidator> logger)
    {
        _dataSource = dataSource;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SessionValidationResult?> ValidateAsync(string token, CancellationToken ct = default)
    {
        var cacheKey = $"legacy-session:{token}";

        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<SessionValidationResult>(cached);

        LegacySessionRow? row;
        try
        {
            row = await _dataSource.FindByTokenAsync(token, _options.SessionTimeoutMinutes, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query sessions table for token validation");
            return null;
        }

        if (row is null)
            return null;

        var result = new SessionValidationResult(
            row.UserId, row.OrgId, row.ContractId,
            row.Email, row.UserName,
            DataSourceId: 1,
            DateTime.UtcNow);

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheMinutes)
            },
            ct);

        return result;
    }
}
