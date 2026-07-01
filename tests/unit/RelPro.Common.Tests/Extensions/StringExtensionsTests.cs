using RelPro.Common.Extensions;

namespace RelPro.Common.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("  ", true)]
    [InlineData("a", false)]
    public void IsNullOrWhiteSpace_VariousInputs(string? input, bool expected) =>
        Assert.Equal(expected, input.IsNullOrWhiteSpace());

    [Fact]
    public void Truncate_ShortString_ReturnsUnchanged() =>
        Assert.Equal("hi", "hi".Truncate(10));

    [Fact]
    public void Truncate_LongString_TruncatesWithSuffix()
    {
        var result = "Hello World".Truncate(8, "…");
        Assert.Equal(8, result.Length);
        Assert.EndsWith("…", result);
    }

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("  Foo  Bar  ", "foo-bar")]
    [InlineData("C# is great!", "c-is-great")]
    public void ToSlug_ProducesLowercaseHyphenated(string input, string expected) =>
        Assert.Equal(expected, input.ToSlug());

    [Theory]
    [InlineData("MyValue", "my_value")]
    [InlineData("ContractId", "contract_id")]
    [InlineData("alreadylower", "alreadylower")]
    public void ToSnakeCase_ConvertsCorrectly(string input, string expected) =>
        Assert.Equal(expected, input.ToSnakeCase());

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("  ", null)]
    [InlineData("value", "value")]
    public void NullIfWhiteSpace_ReturnsNullOrValue(string? input, string? expected) =>
        Assert.Equal(expected, input.NullIfWhiteSpace());
}
