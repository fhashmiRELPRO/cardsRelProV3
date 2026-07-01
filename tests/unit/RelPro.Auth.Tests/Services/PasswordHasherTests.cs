using RelPro.CARDS.Api.Auth.Services;
using System.Security.Cryptography;
using System.Text;

namespace RelPro.Auth.Tests.Services;

public sealed class PasswordHasherTests
{
    private static string GenerateSalt()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string ComputeLegacyHash(string input, string salt)
    {
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(input, saltBytes, 1000, HashAlgorithmName.SHA1);
        return Convert.ToBase64String(pbkdf2.GetBytes(64));
    }

    [Fact]
    public void Verify_ReturnsTrue_WhenPbkdf2PasswordMatches()
    {
        var hasher = new PasswordHasher();
        var salt = GenerateSalt();
        var hash = ComputeLegacyHash("user@test.com" + "correct!", salt);

        Assert.True(hasher.Verify("user@test.com", "correct!", hash, salt));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPasswordWrong()
    {
        var hasher = new PasswordHasher();
        var salt = GenerateSalt();
        var hash = ComputeLegacyHash("user@test.com" + "correct!", salt);

        Assert.False(hasher.Verify("user@test.com", "wrong!", hash, salt));
    }

    [Fact]
    public void Verify_IsCaseSensitive_OnPassword()
    {
        var hasher = new PasswordHasher();
        var salt = GenerateSalt();
        var hash = ComputeLegacyHash("user@test.com" + "Secret1", salt);

        Assert.False(hasher.Verify("user@test.com", "secret1", hash, salt));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenSaltMismatch()
    {
        var hasher = new PasswordHasher();
        var saltA = GenerateSalt();
        var saltB = GenerateSalt();
        var hash = ComputeLegacyHash("user@test.com" + "pass", saltA);

        Assert.False(hasher.Verify("user@test.com", "pass", hash, saltB));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenNullSaltAndHashDoesNotMatch()
    {
        var hasher = new PasswordHasher();

        Assert.False(hasher.Verify("user@test.com", "pass", "notsha256hash", null));
    }
}
