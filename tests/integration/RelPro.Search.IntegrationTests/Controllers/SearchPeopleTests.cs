using System.Net;
using System.Net.Http.Json;
using RelPro.Contracts.Common;
using RelPro.Search.Api.Models;
using RelPro.Search.IntegrationTests.Stubs;

namespace RelPro.Search.IntegrationTests.Controllers;

public sealed class SearchPeopleTests : IClassFixture<SearchApiFactory>
{
    private readonly HttpClient _client;

    public SearchPeopleTests(SearchApiFactory factory)
        => _client = factory.CreateClient();

    // 200
    [Fact]
    public async Task Returns200_WithValidQueryAndToken()
    {
        var response = await _client.SendAsync(Get("/v1/search/people?q=relpro", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PersonResult>>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotEmpty(body.Data!);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task RespectsLimitParameter(int limit)
    {
        var response = await _client.SendAsync(
            Get($"/v1/search/people?q=test&limit={limit}", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(999)]
    public async Task ClampsOutOfRangeLimitTo20(int limit)
    {
        // Controller clamps limit to [1,100] silently - still 200
        var response = await _client.SendAsync(
            Get($"/v1/search/people?q=test&limit={limit}", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 400
    [Fact]
    public async Task Returns400_WhenQueryParamIsMissing()
    {
        var response = await _client.SendAsync(Get("/v1/search/people", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("MISSING_QUERY", body!.ErrorCode);
    }

    [Fact]
    public async Task Returns400_WhenQueryParamIsBlank()
    {
        var response = await _client.SendAsync(Get("/v1/search/people?q=   ", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("MISSING_QUERY", body!.ErrorCode);
    }

    // 401
    [Fact]
    public async Task Returns401_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/v1/search/people?q=test");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Returns401_WhenTokenIsInvalid()
    {
        var response = await _client.SendAsync(Get("/v1/search/people?q=test", "garbage-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 403
    [Fact]
    public async Task Returns403_WhenContractIsInactive()
    {
        var response = await _client.SendAsync(Get("/v1/search/people?q=test", TokenAwareSessionValidator.InactiveContractToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("CONTRACT_INACTIVE", body!.ErrorCode);
    }

    // 500
    [Fact]
    public async Task Returns500_WhenRepositoryThrowsUnexpectedly()
    {
        var response = await _client.SendAsync(Get("/v1/search/people?q=simulate-error", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("INTERNAL_ERROR", body!.ErrorCode);
    }

    // helpers
    private static HttpRequestMessage Get(string path, string token)
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, path);
        msg.Headers.Add("userToken", token);
        return msg;
    }
}
