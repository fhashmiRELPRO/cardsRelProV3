using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RelPro.Infrastructure.Session;
using System.Text.Json;

namespace RelPro.Infrastructure.Tests.Session;

public class LegacySessionValidatorTests
{
    private readonly IDistributedCache _cache = Substitute.For<IDistributedCache>();
    private readonly ILegacySessionDataSource _dataSource = Substitute.For<ILegacySessionDataSource>();
    private readonly LegacySessionOptions _options = new() { CacheMinutes = 5, SessionTimeoutMinutes = 720 };

    private LegacySessionValidator BuildSut() =>
        new(_dataSource, _cache, Options.Create(_options), NullLogger<LegacySessionValidator>.Instance);

    private static LegacySessionRow SampleRow(int userId = 42) =>
        new(userId, OrgId: 5, ContractId: 100, Email: "user@co.com", UserName: "jane.doe");

    [Fact]
    public async Task ValidateAsync_WhenCacheHit_ReturnsCachedResult_WithoutDbCall()
    {
        var result = new SessionValidationResult(1, 2, 3, "a@b.com", "User", 1, DateTime.UtcNow);
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)));

        var validated = await BuildSut().ValidateAsync("token123");

        Assert.NotNull(validated);
        Assert.Equal(1, validated!.UserId);
        await _dataSource.DidNotReceive().FindByTokenAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_WhenCacheMissAndDbReturnsRow_ReturnsPopulatedResult()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        _dataSource.FindByTokenAsync("token-abc", 720, Arg.Any<CancellationToken>())
            .Returns(SampleRow());

        var validated = await BuildSut().ValidateAsync("token-abc");

        Assert.NotNull(validated);
        Assert.Equal(42, validated!.UserId);
        Assert.Equal("user@co.com", validated.Email);
        Assert.Equal(1, validated.DataSourceId);
    }

    [Fact]
    public async Task ValidateAsync_WhenCacheMissAndDbReturnsNull_ReturnsNull()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        _dataSource.FindByTokenAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((LegacySessionRow?)null);

        var validated = await BuildSut().ValidateAsync("expired-token");

        Assert.Null(validated);
    }

    [Fact]
    public async Task ValidateAsync_WhenDbThrows_ReturnsNull()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        _dataSource.FindByTokenAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns<LegacySessionRow?>(_ => throw new Exception("DB offline"));

        var validated = await BuildSut().ValidateAsync("any-token");

        Assert.Null(validated);
    }

    [Fact]
    public async Task ValidateAsync_WhenCacheMissAndSuccess_WritesToCache()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        _dataSource.FindByTokenAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(SampleRow());

        await BuildSut().ValidateAsync("write-cache-token");

        await _cache.Received(1).SetAsync(
            Arg.Is<string>(k => k.Contains("write-cache-token")),
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_CacheKey_HasLegacySessionPrefix()
    {
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        _dataSource.FindByTokenAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(SampleRow());

        await BuildSut().ValidateAsync("my-token");

        await _cache.Received().GetAsync(
            Arg.Is<string>(k => k.StartsWith("legacy-session:")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_PassesConfiguredTimeoutToDataSource()
    {
        var opts = new LegacySessionOptions { SessionTimeoutMinutes = 480, CacheMinutes = 5 };
        var sut = new LegacySessionValidator(_dataSource, _cache, Options.Create(opts), NullLogger<LegacySessionValidator>.Instance);
        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        _dataSource.FindByTokenAsync(Arg.Any<string>(), 480, Arg.Any<CancellationToken>())
            .Returns(SampleRow());

        await sut.ValidateAsync("t");

        await _dataSource.Received(1).FindByTokenAsync(Arg.Any<string>(), 480, Arg.Any<CancellationToken>());
    }
}
