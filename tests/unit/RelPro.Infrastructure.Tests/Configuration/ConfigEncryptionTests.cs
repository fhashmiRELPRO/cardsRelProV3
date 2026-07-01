using Microsoft.Extensions.Configuration;
using RelPro.Infrastructure.Configuration;
using System.Security.Cryptography;

namespace RelPro.Infrastructure.Tests.Configuration;

public class ConfigEncryptionTests
{
    private static readonly byte[] TestKey = RandomNumberGenerator.GetBytes(32);

    [Fact]
    public void EncryptThenDecrypt_ReturnsOriginalValue()
    {
        var plaintext = "Server=db;Pwd=s3cr3t!;";

        var encrypted = ConfigEncryption.Encrypt(TestKey, plaintext);
        var decrypted = ConfigEncryption.Decrypt(TestKey, encrypted["ENC:".Length..]);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesEncPrefix()
    {
        var result = ConfigEncryption.Encrypt(TestKey, "any value");

        Assert.StartsWith("ENC:", result);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertextEachCall()
    {
        var a = ConfigEncryption.Encrypt(TestKey, "same");
        var b = ConfigEncryption.Encrypt(TestKey, "same");

        // Different nonces → different ciphertext (authenticated encryption)
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsCryptographicException()
    {
        var encrypted = ConfigEncryption.Encrypt(TestKey, "secret");
        var wrongKey = RandomNumberGenerator.GetBytes(32);

        Assert.Throws<AuthenticationTagMismatchException>(
            () => ConfigEncryption.Decrypt(wrongKey, encrypted["ENC:".Length..]));
    }

    [Fact]
    public void Decrypt_WithTamperedPayload_ThrowsCryptographicException()
    {
        var encrypted = ConfigEncryption.Encrypt(TestKey, "secret");
        var payload = encrypted["ENC:".Length..];
        var bytes = Convert.FromBase64String(payload);
        bytes[^1] ^= 0xFF; // flip last byte of ciphertext

        Assert.Throws<AuthenticationTagMismatchException>(
            () => ConfigEncryption.Decrypt(TestKey, Convert.ToBase64String(bytes)));
    }

    [Fact]
    public void AddEncryptedConfiguration_DecryptsEncPrefixedValues()
    {
        var encryptedConnStr = ConfigEncryption.Encrypt(TestKey, "Server=db;Pwd=s3cr3t!;");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MySql:ConnectionString"] = encryptedConnStr,
                ["Logging:Level"] = "Information",
            })
            .AddEncryptedConfiguration(TestKey)
            .Build();

        Assert.Equal("Server=db;Pwd=s3cr3t!;", config["MySql:ConnectionString"]);
        Assert.Equal("Information", config["Logging:Level"]);
    }

    [Fact]
    public void AddEncryptedConfiguration_LeavesPlaintextValuesUntouched()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Key"] = "plaintext-value",
            })
            .AddEncryptedConfiguration(TestKey)
            .Build();

        Assert.Equal("plaintext-value", config["Key"]);
    }

    [Fact]
    public void AddEncryptedConfiguration_NoEncValues_DoesNotRequireKey()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Key"] = "plaintext",
            })
            .AddEncryptedConfiguration(key: null)
            .Build();

        Assert.Equal("plaintext", config["Key"]);
    }

    [Fact]
    public void AddEncryptedConfiguration_HasEncValues_NullKey_Throws()
    {
        var encryptedValue = ConfigEncryption.Encrypt(TestKey, "secret");

        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Secret"] = encryptedValue,
            });

        Assert.Throws<InvalidOperationException>(() =>
            builder.AddEncryptedConfiguration(key: null).Build());
    }
}
