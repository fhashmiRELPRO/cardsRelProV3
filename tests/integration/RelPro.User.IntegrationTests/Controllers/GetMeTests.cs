using System.Net;
using System.Net.Http.Json;
using RelPro.Contracts.Common;
using RelPro.User.Api.Models;
using RelPro.User.IntegrationTests.Stubs;

namespace RelPro.User.IntegrationTests.Controllers;

public sealed class GetMeTests : IClassFixture<UserApiFactory>
{
    private readonly HttpClient _client;

    public GetMeTests(UserApiFactory factory)
        => _client = factory.CreateClient();

    // 200
    [Fact]
    public async Task Returns200_WithValidToken()
    {
        var response = await _client.SendAsync(Get("/v1/user/me", TokenAwareSessionValidator.ValidToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileResponse>>();
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.Equal("test@relpro.com", body.Data!.Email);
        Assert.Equal(10, body.Data.OrgId);
        Assert.True(body.Data.IsContractActive);
    }

    [Fact]
    public async Task ReturnsUserEmail_FromSessionContext()
    {
        var response = await _client.SendAsync(Get("/v1/user/me", TokenAwareSessionValidator.ValidToken));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileResponse>>();

        Assert.Equal(1001, body!.Data!.UserId);
        Assert.Equal("Test User", body.Data.DisplayName);
    }

    // 401
    [Fact]
    public async Task Returns401_WhenNoTokenProvided()
    {
        var response = await _client.GetAsync("/v1/user/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("MISSING_TOKEN", body!.ErrorCode);
    }

    [Fact]
    public async Task Returns401_WhenTokenIsInvalid()
    {
        var response = await _client.SendAsync(Get("/v1/user/me", "not-a-real-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("INVALID_SESSION", body!.ErrorCode);
    }

    // 403
    [Fact]
    public async Task Returns403_WhenContractIsInactive()
    {
        var response = await _client.SendAsync(Get("/v1/user/me", TokenAwareSessionValidator.InactiveContractToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.Equal("CONTRACT_INACTIVE", body!.ErrorCode);
    }

    // helpers
    private static HttpRequestMessage Get(string path, string token)
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, path);
        msg.Headers.Add("userToken", token);
        return msg;
    }
}
