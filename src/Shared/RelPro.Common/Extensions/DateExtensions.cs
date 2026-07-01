namespace RelPro.Common.Extensions;

public static class DateExtensions
{
    public static long ToUnixSeconds(this DateTime dt) =>
        new DateTimeOffset(dt.ToUniversalTime()).ToUnixTimeSeconds();

    public static long ToUnixSeconds(this DateTimeOffset dto) =>
        dto.ToUnixTimeSeconds();

    public static bool IsExpired(this DateOnly? expiryDate) =>
        expiryDate.HasValue && expiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    public static bool IsExpired(this DateOnly expiryDate) =>
        expiryDate < DateOnly.FromDateTime(DateTime.UtcNow);

    public static bool IsExpired(this DateTime expiryDate) =>
        expiryDate < DateTime.UtcNow;

    public static DateTime StartOfDay(this DateTime dt) =>
        dt.Date;

    public static DateTime EndOfDay(this DateTime dt) =>
        dt.Date.AddDays(1).AddTicks(-1);

    public static DateOnly ToDateOnly(this DateTime dt) =>
        DateOnly.FromDateTime(dt);
}
