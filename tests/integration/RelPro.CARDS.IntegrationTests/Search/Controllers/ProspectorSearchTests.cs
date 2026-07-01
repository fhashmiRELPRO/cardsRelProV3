using System.Net;
using System.Net.Http.Json;
using RelPro.Contracts.Common;
using RelPro.CARDS.Api.Search.Models;
using RelPro.CARDS.IntegrationTests.Stubs;

namespace RelPro.CARDS.IntegrationTests.Search.Controllers;

public sealed class ProspectorSearchTests : IClassFixture<CardsApiFactory>
{
    private readonly HttpClient _client;

    public ProspectorSearchTests(CardsApiFactory factory)
        => _client = factory.CreateClient();

    // 200
    [Fact]
    public async Task Returns200_WithQueryParam()
    {
        var response = await _client.SendAsync(Get("/v1/search?query=relpro", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProspectorSearchResponse>();
        Assert.NotNull(body);
        Assert.True(body.TotalIndividuals > 0);
        Assert.NotEmpty(body.Individuals);
        Assert.True(body.SearchId > 0);
    }

    [Fact]
    public async Task Returns200_WithFirstNameAndLastName()
    {
        var response = await _client.SendAsync(
            Get("/v1/search?firstName=Jane&lastName=Smith", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResponseShapeIsWireCompatibleWithCardsLegacy()
    {
        var response = await _client.SendAsync(Get("/v1/search?query=test", TokenAwareSessionValidator.ValidToken));
        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"totalIndividuals\"", json);
        Assert.Contains("\"individuals\"", json);
        Assert.Contains("\"searchId\"", json);
    }

    // 400
    [Fact]
    public async Task Returns400_WhenNoSearchTermProvided()
    {
        var response = await _client.SendAsync(Get("/v1/search", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("MISSING_SEARCH_TERM", body!.ErrorCode);
    }

    [Fact]
    public async Task Returns400_WhenAllSearchTermsAreBlank()
    {
        var response = await _client.SendAsync(
            Get("/v1/search?query=   &firstName=   ", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/v1/search?pageSize=0&query=test")]
    [InlineData("/v1/search?pageSize=200&query=test")]
    [InlineData("/v1/search?page=0&query=test")]
    public async Task HandlesInvalidPaginationGracefully(string path)
    {
        var response = await _client.SendAsync(Get(path, TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 401
    [Fact]
    public async Task Returns401_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/v1/search?query=test");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Returns401_WhenTokenIsInvalid()
    {
        var response = await _client.SendAsync(Get("/v1/search?query=test", "bad-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 408
    [Fact]
    public async Task Returns408_WhenAtlasSearchTimesOut()
    {
        var response = await _client.SendAsync(Get("/v1/search?query=simulate-timeout", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.RequestTimeout, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("REQUEST_TIMEOUT", body!.ErrorCode);
    }

    private static HttpRequestMessage Get(string path, string token)
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, path);
        msg.Headers.Add("userToken", token);
        return msg;
    }
}
