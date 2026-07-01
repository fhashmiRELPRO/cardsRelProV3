using RelPro.Contracts.Common;

namespace RelPro.Contracts.Tests.Common;

public class PaginationRequestTests
{
    [Fact]
    public void Offset_CalculatesCorrectly()
    {
        var request = new PaginationRequest { Page = 3, PageSize = 20 };

        Assert.Equal(40, request.Offset);
    }

    [Fact]
    public void Offset_OnFirstPage_IsZero()
    {
        var request = new PaginationRequest { Page = 1, PageSize = 20 };

        Assert.Equal(0, request.Offset);
    }

    [Fact]
    public void Clamp_WhenPageBelowOne_SetsToOne()
    {
        var request = new PaginationRequest { Page = -5, PageSize = 20 };
        var clamped = request.Clamp();

        Assert.Equal(1, clamped.Page);
    }

    [Fact]
    public void Clamp_WhenPageSizeExceedsMax_ClampsToMax()
    {
        var request = new PaginationRequest { Page = 1, PageSize = 500 };
        var clamped = request.Clamp(maxPageSize: 100);

        Assert.Equal(100, clamped.PageSize);
    }

    [Fact]
    public void Clamp_WhenPageSizeIsZero_SetsToOne()
    {
        var request = new PaginationRequest { Page = 1, PageSize = 0 };
        var clamped = request.Clamp();

        Assert.Equal(1, clamped.PageSize);
    }

    [Fact]
    public void Defaults_ArePageOneAndTwenty()
    {
        var request = new PaginationRequest();

        Assert.Equal(1, request.Page);
        Assert.Equal(20, request.PageSize);
    }
}
