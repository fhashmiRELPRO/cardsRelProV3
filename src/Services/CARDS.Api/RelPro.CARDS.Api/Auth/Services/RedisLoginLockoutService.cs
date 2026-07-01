using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RelPro.CARDS.Api.Auth.Options;
using RelPro.Infrastructure.Email;

namespace RelPro.CARDS.Api.Auth.Services;

public sealed class RedisLoginLockoutService : ILoginLockoutService
{
    private readonly IDistributedCache _cache;
    private readonly ContractOptions _contractOpts;
    private readonly EmailOptions _emailOpts;
    private readonly IEmailService _email;
    private readonly ILogger<RedisLoginLockoutService> _logger;

    public RedisLoginLockoutService(
        IDistributedCache cache,
        IOptions<ContractOptions> contractOpts,
        IOptions<EmailOptions> emailOpts,
        IEmailService email,
        ILogger<RedisLoginLockoutService> logger)
    {
        _cache        = cache;
        _contractOpts = contractOpts.Value;
        _emailOpts    = emailOpts.Value;
        _email        = email;
        _logger       = logger;
    }

    public async Task<bool> IsLockedAsync(string email, CancellationToken ct = default)
    {
        var val = await _cache.GetStringAsync(Key(email), ct);
        return val is not null && int.TryParse(val, out var count) && count >= _contractOpts.FailureLimit;
    }

    public async Task RecordFailureAsync(string email, CancellationToken ct = default)
    {
        var val = await _cache.GetStringAsync(Key(email), ct);
        var count = val is not null && int.TryParse(val, out var c) ? c + 1 : 1;

        await _cache.SetStringAsync(Key(email), count.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_contractOpts.FailureTimeMinutes)
            }, ct);

        if (count == _emailOpts.LoginFailureLimit)
        {
            try
            {
                await _email.SendLoginFailureAlertAsync(email, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failure alert email failed for {Email}", email);
            }
        }
    }

    public Task ClearAsync(string email, CancellationToken ct = default) =>
        _cache.RemoveAsync(Key(email), ct);

    private static string Key(string email) => $"login:lock:{email}";
}
