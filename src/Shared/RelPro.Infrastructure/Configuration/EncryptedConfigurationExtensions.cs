using Microsoft.Extensions.Configuration;

namespace RelPro.Infrastructure.Configuration;

public static class EncryptedConfigurationExtensions
{
    private const string EncPrefix = "ENC:";

    /// <summary>
    /// Decrypts any configuration value prefixed with "ENC:" using AES-256-GCM.
    /// The master key is read from the RELPRO_CONFIG_KEY environment variable.
    /// If no ENC: values are present the key is not required (safe for local dev with user-secrets).
    /// </summary>
    public static IConfigurationBuilder AddEncryptedConfiguration(this IConfigurationBuilder builder)
    {
        var key = ConfigEncryption.LoadKeyFromEnvironment();
        return builder.AddEncryptedConfiguration(key);
    }

    /// <summary>
    /// Decrypts any configuration value prefixed with "ENC:" using the supplied key.
    /// Pass null to allow startup when no encrypted values are present (dev/test scenarios).
    /// </summary>
    public static IConfigurationBuilder AddEncryptedConfiguration(
        this IConfigurationBuilder builder, byte[]? key)
    {
        var snapshot = builder.Build();
        var encryptedPairs = snapshot
            .AsEnumerable()
            .Where(kv => kv.Value?.StartsWith(EncPrefix) == true)
            .ToList();

        if (encryptedPairs.Count == 0)
            return builder;

        if (key is null)
            throw new InvalidOperationException(
                $"Configuration contains {encryptedPairs.Count} encrypted value(s) but RELPRO_CONFIG_KEY is not set. " +
                "Set this environment variable to the base64-encoded 32-byte master key.");

        var decrypted = encryptedPairs.ToDictionary(
            kv => kv.Key,
            kv => (string?)ConfigEncryption.Decrypt(key, kv.Value![EncPrefix.Length..]));

        return builder.AddInMemoryCollection(decrypted);
    }
}
