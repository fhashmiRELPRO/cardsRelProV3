using System.Security.Cryptography;
using System.Text;

namespace RelPro.Infrastructure.Configuration;

/// <summary>
/// AES-256-GCM encrypt/decrypt for configuration values.
///
/// Format: ENC:{base64(nonce[12] + tag[16] + ciphertext)}
///
/// To encrypt a value for appsettings.json:
///   var key = Convert.FromBase64String(Environment.GetEnvironmentVariable("RELPRO_CONFIG_KEY")!);
///   var encrypted = ConfigEncryption.Encrypt(key, "my-secret-value");
///   // paste the result into appsettings.json
///
/// To generate a new master key (run once per environment):
///   var key = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
///   Console.WriteLine(Convert.ToBase64String(key));
///   // store this in RELPRO_CONFIG_KEY environment variable on the server
/// </summary>
public static class ConfigEncryption
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public static string Encrypt(byte[] key, string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var payload = new byte[NonceSize + TagSize + ciphertext.Length];
        nonce.CopyTo(payload, 0);
        tag.CopyTo(payload, NonceSize);
        ciphertext.CopyTo(payload, NonceSize + TagSize);

        return "ENC:" + Convert.ToBase64String(payload);
    }

    public static string Decrypt(byte[] key, string base64Payload)
    {
        var payload = Convert.FromBase64String(base64Payload);
        var nonce = payload[..NonceSize];
        var tag = payload[NonceSize..(NonceSize + TagSize)];
        var ciphertext = payload[(NonceSize + TagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    public static byte[]? LoadKeyFromEnvironment(string envVar = "RELPRO_CONFIG_KEY")
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        return string.IsNullOrWhiteSpace(value) ? null : Convert.FromBase64String(value);
    }
}
