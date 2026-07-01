using System.Net;
using System.Net.Http.Json;
using RelPro.Contracts.Common;
using RelPro.CARDS.Api.User.Models;
using RelPro.CARDS.IntegrationTests.Stubs;

namespace RelPro.CARDS.IntegrationTests.User.Controllers;

public sealed class GetByIdTests : IClassFixture<CardsApiFactory>
{
    private readonly HttpClient _client;

    public GetByIdTests(CardsApiFactory factory)
        => _client = factory.CreateClient();

    // 200
    [Fact]
    public async Task Returns200_WhenUserExistsInSameOrg()
    {
        var response = await _client.SendAsync(Get("/v1/user/1001", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDetailResponse>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.Equal(1001, body.Data!.UserId);
        Assert.Equal("jane@relpro.com", body.Data.Email);
        Assert.Equal(10, body.Data.OrgId);
    }

    [Fact]
    public async Task ResponseIncludesAllExpectedFields()
    {
        var response = await _client.SendAsync(Get("/v1/user/1001", TokenAwareSessionValidator.ValidToken));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDetailResponse>>();
        var data = body!.Data!;

        Assert.Equal("Jane", data.FirstName);
        Assert.Equal("Smith", data.LastName);
        Assert.Equal("Product Manager", data.Headline);
        Assert.True(data.IsActive);
        Assert.True(data.IsEnabled);
        Assert.False(data.IsAdmin);
        Assert.Equal("Test Bank", data.OrgName);
    }

    // 400
    [Fact]
    public async Task Returns400_WhenIdIsZero()
    {
        var response = await _client.SendAsync(Get("/v1/user/0", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("INVALID_ID", body!.ErrorCode);
    }

    [Fact]
    public async Task Returns400_WhenIdIsNegative()
    {
        var response = await _client.SendAsync(Get("/v1/user/-5", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("INVALID_ID", body!.ErrorCode);
    }

    [Theory]
    [InlineData("/v1/user/abc")]
    [InlineData("/v1/user/1.5")]
    [InlineData("/v1/user/")]
    public async Task Returns404_WhenIdIsNotAnInteger(string path)
    {
        var response = await _client.SendAsync(Get(path, TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 401
    [Fact]
    public async Task Returns401_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/v1/user/1001");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Returns401_WhenTokenIsInvalid()
    {
        var response = await _client.SendAsync(Get("/v1/user/1001", "bogus-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 403
    [Fact]
    public async Task Returns403_WhenEntitlementIsMissing()
    {
        var response = await _client.SendAsync(Get("/v1/user/1001", TokenAwareSessionValidator.NoEntitlementToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("ENTITLEMENT_REQUIRED", body!.ErrorCode);
    }

    [Fact]
    public async Task Returns403_WhenContractIsInactive()
    {
        var response = await _client.SendAsync(Get("/v1/user/1001", TokenAwareSessionValidator.InactiveContractToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("CONTRACT_INACTIVE", body!.ErrorCode);
    }

    // 404
    [Fact]
    public async Task Returns404_WhenUserDoesNotExist()
    {
        var response = await _client.SendAsync(Get("/v1/user/9999", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("NOT_FOUND", body!.ErrorCode);
    }

    [Fact]
    public async Task Returns404_WhenUserBelongsToDifferentOrg()
    {
        var response = await _client.SendAsync(Get("/v1/user/1099", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("NOT_FOUND", body!.ErrorCode);
    }

    // 500
    [Fact]
    public async Task Returns500_WhenRepositoryThrowsUnexpectedly()
    {
        var response = await _client.SendAsync(Get("/v1/user/500", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("INTERNAL_ERROR", body!.ErrorCode);
    }

    private static HttpRequestMessage Get(string path, string token)
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, path);
        msg.Headers.Add("userToken", token);
        return msg;
    }
}
