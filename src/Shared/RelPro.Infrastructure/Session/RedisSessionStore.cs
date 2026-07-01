using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace RelPro.Infrastructure.Session;

public sealed class RedisSessionStore : ISessionStore
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(8);
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public RedisSessionStore(IDistributedCache cache) => _cache = cache;

    public async Task<string> CreateAsync(FullSessionData data, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var json = JsonSerializer.Serialize(data);
        await _cache.SetStringAsync(
            CacheKey(token),
            json,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl },
            ct);
        return token;
    }

    public async Task<FullSessionData?> GetAsync(string token, CancellationToken ct = default)
    {
        var json = await _cache.GetStringAsync(CacheKey(token), ct);
        return json is null ? null : JsonSerializer.Deserialize<FullSessionData>(json, JsonOpts);
    }

    public Task InvalidateAsync(string token, CancellationToken ct = default) =>
        _cache.RemoveAsync(CacheKey(token), ct);

    private static string CacheKey(string token) => $"session:{token}";
}
