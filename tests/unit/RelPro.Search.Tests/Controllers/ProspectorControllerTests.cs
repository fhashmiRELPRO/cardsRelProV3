using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using RelPro.Search.Api.Controllers;
using RelPro.Search.Api.Models;
using RelPro.Search.Api.Repositories;

namespace RelPro.Search.Tests.Controllers;

public sealed class ProspectorControllerTests
{
    private readonly IProspectorSearchRepository _repo = Substitute.For<IProspectorSearchRepository>();
    private ProspectorController BuildController() => new(_repo);

    private static IndividualResult SampleIndividual(string first = "Jane", string last = "Doe") => new()
    {
        FirstName = first, LastName = last, Role = "CEO", Company = "Acme", CurrentFlag = 1
    };

    [Fact]
    public async Task Search_Returns400_WhenNoSearchTermsProvided()
    {
        var result = await BuildController().Search(new ProspectorSearchRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Search_Returns200_WhenQueryProvided()
    {
        var response = new ProspectorSearchResponse
        {
            TotalIndividuals = 1,
            Individuals = [SampleIndividual()]
        };
        _repo.SearchAsync(Arg.Any<ProspectorSearchRequest>(), Arg.Any<CancellationToken>())
             .Returns(response);

        var result = await BuildController().Search(
            new ProspectorSearchRequest { Query = "relpro" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<ProspectorSearchResponse>(ok.Value);
        Assert.Equal(1, body.TotalIndividuals);
        Assert.Single(body.Individuals);
    }

    [Fact]
    public async Task Search_Returns200_WhenFirstNameAndLastNameProvided()
    {
        _repo.SearchAsync(Arg.Any<ProspectorSearchRequest>(), Arg.Any<CancellationToken>())
             .Returns(ProspectorSearchResponse.Empty);

        var result = await BuildController().Search(
            new ProspectorSearchRequest { FirstName = "Jane", LastName = "Doe" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Search_PassesRequestToRepository()
    {
        _repo.SearchAsync(Arg.Any<ProspectorSearchRequest>(), Arg.Any<CancellationToken>())
             .Returns(ProspectorSearchResponse.Empty);
        var request = new ProspectorSearchRequest
        {
            Query = "relpro", PageSize = 100, Page = 1, ShowPastRole = true, IncludeInActiveCompanies = true
        };

        await BuildController().Search(request, CancellationToken.None);

        await _repo.Received(1).SearchAsync(
            Arg.Is<ProspectorSearchRequest>(r =>
                r.Query == "relpro" && r.PageSize == 100 && r.ShowPastRole),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_ResponseMatchesCARDSShape()
    {
        var response = new ProspectorSearchResponse
        {
            TotalIndividuals = 42,
            Individuals = [SampleIndividual("Elon", "Musk")],
            SearchId = 0
        };
        _repo.SearchAsync(Arg.Any<ProspectorSearchRequest>(), Arg.Any<CancellationToken>())
             .Returns(response);

        var result = await BuildController().Search(
            new ProspectorSearchRequest { Query = "test" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<ProspectorSearchResponse>(ok.Value);

        // Verify CARDS-compatible field names are present via model shape
        Assert.Equal(42, body.TotalIndividuals);
        Assert.Equal(0, body.SearchId);
        Assert.Equal("Elon", body.Individuals[0].FirstName);
        Assert.Equal("Musk", body.Individuals[0].LastName);
    }
}
