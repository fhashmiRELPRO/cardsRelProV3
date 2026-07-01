using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using RelPro.Contracts.Common;
using RelPro.CARDS.Api.Search.Controllers;
using RelPro.CARDS.Api.Search.Models;
using RelPro.CARDS.Api.Search.Repositories;

namespace RelPro.Search.Tests.Controllers;

public sealed class SearchControllerTests
{
    private readonly IPersonSearchRepository _repo = Substitute.For<IPersonSearchRepository>();

    private SearchController BuildController() => new(_repo);

    [Fact]
    public async Task SearchPeople_Returns400_WhenQueryIsEmpty()
    {
        var result = await BuildController().SearchPeople(null);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var body = Assert.IsType<ApiResponse>(bad.Value);
        Assert.False(body.Success);
        Assert.Equal("MISSING_QUERY", body.ErrorCode);
    }

    [Fact]
    public async Task SearchPeople_Returns400_WhenQueryIsWhitespace()
    {
        var result = await BuildController().SearchPeople("   ");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SearchPeople_Returns200_WithResults()
    {
        var people = new List<PersonResult>
        {
            new(1, "Elon", "Musk", null, "Tesla", "CEO")
        };
        _repo.SearchAsync("Elon", 20, Arg.Any<CancellationToken>())
             .Returns(people);

        var result = await BuildController().SearchPeople("Elon");

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<ApiResponse<IReadOnlyList<PersonResult>>>(ok.Value);
        Assert.True(body.Success);
        Assert.Single(body.Data!);
        Assert.Equal("Elon", body.Data[0].FirstName);
    }

    [Fact]
    public async Task SearchPeople_ClampsLimit_To20WhenOutOfRange()
    {
        _repo.SearchAsync(Arg.Any<string>(), 20, Arg.Any<CancellationToken>())
             .Returns(new List<PersonResult>());

        await BuildController().SearchPeople("test", limit: 0);
        await BuildController().SearchPeople("test", limit: 999);

        await _repo.Received(2).SearchAsync(Arg.Any<string>(), 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchPeople_PassesQuery_ToRepository()
    {
        _repo.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(new List<PersonResult>());

        await BuildController().SearchPeople("Bill Gates");

        await _repo.Received(1).SearchAsync("Bill Gates", 20, Arg.Any<CancellationToken>());
    }
}
