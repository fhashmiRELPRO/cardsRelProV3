using NSubstitute;
using RelPro.Common.Exceptions;
using RelPro.CARDS.Api.User.Repositories;
using RelPro.CARDS.Api.User.Services;

namespace RelPro.User.Tests.Services;

public sealed class UserServiceTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private UserService BuildService() => new(_repo);

    private static UserRow SampleRow(int orgId = 5) => new()
    {
        Id = 42, Name = "Jane Doe", Email = "jane@acme.com",
        OrganizationId = orgId, OrganizationName = "Acme Corp",
        FirstName = "Jane", LastName = "Doe", Headline = "CEO",
        Active = true, Enabled = true, IsAdmin = false,
        DateLicenseExpires = "2027-12-31"
    };

    [Fact]
    public async Task GetById_ReturnsUserDetail_WhenFoundInSameOrg()
    {
        _repo.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns(SampleRow(orgId: 5));

        var result = await BuildService().GetByIdAsync(requestingOrgId: 5, userId: 42);

        Assert.Equal(42, result.UserId);
        Assert.Equal("Jane Doe", result.Name);
        Assert.Equal("jane@acme.com", result.Email);
        Assert.Equal(5, result.OrgId);
        Assert.Equal("Acme Corp", result.OrgName);
        Assert.True(result.IsActive);
        Assert.False(result.IsAdmin);
    }

    [Fact]
    public async Task GetById_ThrowsResourceNotFound_WhenUserDoesNotExist()
    {
        _repo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((UserRow?)null);

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => BuildService().GetByIdAsync(requestingOrgId: 5, userId: 99));
    }

    [Fact]
    public async Task GetById_ThrowsResourceNotFound_WhenUserBelongsToDifferentOrg()
    {
        _repo.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns(SampleRow(orgId: 99));

        // Requesting org is 5, but user belongs to org 99 - must not reveal existence
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => BuildService().GetByIdAsync(requestingOrgId: 5, userId: 42));
    }

    [Fact]
    public async Task GetById_MapsAllFields_FromRow()
    {
        _repo.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns(SampleRow(orgId: 5));

        var result = await BuildService().GetByIdAsync(requestingOrgId: 5, userId: 42);

        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("CEO", result.Headline);
        Assert.True(result.IsEnabled);
        Assert.Equal("2027-12-31", result.LicenseExpires);
    }
}
