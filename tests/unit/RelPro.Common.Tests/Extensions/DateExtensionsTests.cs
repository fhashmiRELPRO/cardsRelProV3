using RelPro.Common.Extensions;

namespace RelPro.Common.Tests.Extensions;

public class DateExtensionsTests
{
    [Fact]
    public void ToUnixSeconds_KnownDate_ReturnsCorrectValue()
    {
        var dt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(1704067200L, dt.ToUnixSeconds());
    }

    [Fact]
    public void IsExpired_NullDate_ReturnsFalse() =>
        Assert.False(((DateOnly?)null).IsExpired());

    [Fact]
    public void IsExpired_FutureDate_ReturnsFalse() =>
        Assert.False(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).IsExpired());

    [Fact]
    public void IsExpired_PastDate_ReturnsTrue() =>
        Assert.True(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).IsExpired());

    [Fact]
    public void StartOfDay_ClearsTimeComponent()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);
        Assert.Equal(new DateTime(2024, 6, 15, 0, 0, 0), dt.StartOfDay());
    }

    [Fact]
    public void EndOfDay_IsLastTickOfDay()
    {
        var dt = new DateTime(2024, 6, 15, 1, 0, 0);
        var end = dt.EndOfDay();
        Assert.Equal(new DateTime(2024, 6, 15), end.Date);
        Assert.Equal(new DateTime(2024, 6, 16).AddTicks(-1), end);
    }
}
