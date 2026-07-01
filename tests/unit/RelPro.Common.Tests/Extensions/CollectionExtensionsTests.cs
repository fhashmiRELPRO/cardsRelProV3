using RelPro.Common.Extensions;

namespace RelPro.Common.Tests.Extensions;

public class CollectionExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_NullCollection_ReturnsTrue() =>
        Assert.True(((IEnumerable<int>?)null).IsNullOrEmpty());

    [Fact]
    public void IsNullOrEmpty_EmptyCollection_ReturnsTrue() =>
        Assert.True(Array.Empty<int>().IsNullOrEmpty());

    [Fact]
    public void IsNullOrEmpty_NonEmptyCollection_ReturnsFalse() =>
        Assert.False(new[] { 1 }.IsNullOrEmpty());

    [Fact]
    public void ToPagedResult_SetsAllProperties()
    {
        var items = new[] { "a", "b", "c" };
        var result = items.ToPagedResult(page: 2, pageSize: 10, totalCount: 25);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void ToPagedResult_WithMap_ProjectsItems()
    {
        var numbers = new[] { 1, 2, 3 };
        var result = numbers.ToPagedResult(n => n.ToString(), page: 1, pageSize: 10, totalCount: 3);

        Assert.Equal(["1", "2", "3"], result.Items);
    }
}
