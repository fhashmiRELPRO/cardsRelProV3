using Microsoft.Extensions.Configuration;

namespace RelPro.Infrastructure.Configuration;

public static class EncryptedConfigurationExtensions
{
    private const string EncPrefix = "ENC:";

    public static IConfigurationBuilder AddEncryptedConfiguration(this IConfigurationBuilder builder)
    {
        var key = ConfigEncryption.LoadKeyFromEnvironment();
        return builder.AddEncryptedConfiguration(key);
    }

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
