using RelPro.Contracts.Common;

namespace RelPro.Contracts.Tests.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_WhenTotalCountAndPageSizeSet_CalculatesCorrectly()
    {
        var result = new PagedResult<int> { TotalCount = 50, PageSize = 20, Page = 1 };

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenTotalCountIsExactMultiple_HasNoExtraPage()
    {
        var result = new PagedResult<int> { TotalCount = 40, PageSize = 20, Page = 1 };

        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenPageSizeIsZero_ReturnsZero()
    {
        var result = new PagedResult<int> { TotalCount = 50, PageSize = 0, Page = 1 };

        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void HasNextPage_WhenNotOnLastPage_ReturnsTrue()
    {
        var result = new PagedResult<int> { TotalCount = 50, PageSize = 20, Page = 1 };

        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_WhenOnLastPage_ReturnsFalse()
    {
        var result = new PagedResult<int> { TotalCount = 50, PageSize = 20, Page = 3 };

        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void HasPreviousPage_WhenOnFirstPage_ReturnsFalse()
    {
        var result = new PagedResult<int> { TotalCount = 50, PageSize = 20, Page = 1 };

        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_WhenNotOnFirstPage_ReturnsTrue()
    {
        var result = new PagedResult<int> { TotalCount = 50, PageSize = 20, Page = 2 };

        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void Empty_ReturnsEmptyResultWithDefaults()
    {
        var result = PagedResult<string>.Empty();

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }
}
