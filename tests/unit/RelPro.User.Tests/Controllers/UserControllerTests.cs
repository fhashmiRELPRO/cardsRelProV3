using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RelPro.Common.Exceptions;
using RelPro.Contracts.Common;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Testing;
using RelPro.User.Api.Controllers;
using RelPro.User.Api.Models;
using RelPro.User.Api.Services;

namespace RelPro.User.Tests.Controllers;

public sealed class UserControllerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();

    private static IConfiguration EmptyConfig() => new ConfigurationBuilder().Build();

    private UserController BuildController() =>
        new(FakeRequestContext.Default, _userService, EmptyConfig());

    private static UserDetailResponse SampleUser() => new(
        UserId: 42, Name: "Jane Doe", Email: "jane@acme.com",
        OrgId: 5, OrgName: "Acme", FirstName: "Jane", LastName: "Doe",
        Headline: "CEO", IsActive: true, IsEnabled: true, IsAdmin: false,
        LicenseExpires: "2027-12-31");

    // --- Me() ---

    [Fact]
    public void Me_Returns200_WithContextData()
    {
        var ctx = FakeRequestContext.Default;
        var controller = new UserController(ctx, _userService, EmptyConfig());

        var result = controller.Me() as OkObjectResult;

        Assert.NotNull(result);
        var body = Assert.IsType<ApiResponse<UserProfileResponse>>(result.Value);
        Assert.True(body.Success);
        Assert.Equal(ctx.UserId, body.Data!.UserId);
        Assert.Equal(ctx.OrgId, body.Data.OrgId);
        Assert.Equal(ctx.ContractId, body.Data.ContractId);
        Assert.Equal(ctx.UserEmail, body.Data.Email);
        Assert.Equal(ctx.UserName, body.Data.DisplayName);
        Assert.Equal(ctx.IsContractActive, body.Data.IsContractActive);
    }

    [Fact]
    public void Me_ReturnsContractStatus_AsString()
    {
        var ctx = FakeRequestContext.Default;
        var controller = new UserController(ctx, _userService, EmptyConfig());

        var result = (controller.Me() as OkObjectResult)!.Value as ApiResponse<UserProfileResponse>;

        Assert.Equal("Active", result!.Data!.ContractStatus);
    }

    [Fact]
    public void Me_ReturnsExpiry_WhenSet()
    {
        var ctx = FakeRequestContext.Default;
        var controller = new UserController(ctx, _userService, EmptyConfig());

        var result = (controller.Me() as OkObjectResult)!.Value as ApiResponse<UserProfileResponse>;

        Assert.NotNull(result!.Data!.ContractExpiry);
    }

    // --- GetById() ---

    [Fact]
    public async Task GetById_Returns200_WithUserDetail()
    {
        _userService.GetByIdAsync(Arg.Any<int>(), 42, Arg.Any<CancellationToken>())
                    .Returns(SampleUser());

        var result = await BuildController().GetById(42, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<ApiResponse<UserDetailResponse>>(ok.Value);
        Assert.True(body.Success);
        Assert.Equal(42, body.Data!.UserId);
        Assert.Equal("Jane Doe", body.Data.Name);
    }

    [Fact]
    public async Task GetById_PassesOrgIdAndUserIdToService()
    {
        _userService.GetByIdAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                    .Returns(SampleUser());

        var ctx = FakeRequestContext.Default;
        var controller = new UserController(ctx, _userService, EmptyConfig());
        await controller.GetById(42, CancellationToken.None);

        await _userService.Received(1).GetByIdAsync(ctx.OrgId, 42, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_BubblesNotFoundException_ToGlobalHandler()
    {
        _userService.GetByIdAsync(Arg.Any<int>(), 99, Arg.Any<CancellationToken>())
                    .ThrowsAsync(new ResourceNotFoundException("User", 99));

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => BuildController().GetById(99, CancellationToken.None));
    }
}
