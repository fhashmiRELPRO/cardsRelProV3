using System.Security.Cryptography;
using System.Text;

namespace RelPro.Auth.Api.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 1000;
    private const int HashBytes = 64;

    public bool Verify(string email, string password, string storedHash, string? storedSalt)
    {
        var input = email + password;

        if (storedSalt is not null)
            return VerifyPbkdf2(input, storedHash, storedSalt);

        // Legacy SHA-256 fallback (accounts created before PBKDF2 migration)
        return VerifySha256(input, storedHash);
    }

    private static bool VerifyPbkdf2(string input, string storedHash, string salt)
    {
        // Legacy used Rfc2898DeriveBytes default = HMACSHA1
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(input, saltBytes, Iterations, HashAlgorithmName.SHA1);
        var computed = Convert.ToBase64String(pbkdf2.GetBytes(HashBytes));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(storedHash));
    }

    private static bool VerifySha256(string input, string storedHash)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var computed = Convert.ToBase64String(hashBytes);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
