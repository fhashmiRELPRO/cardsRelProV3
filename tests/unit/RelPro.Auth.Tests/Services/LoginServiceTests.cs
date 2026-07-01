using NSubstitute;
using RelPro.Auth.Api.Models;
using RelPro.Auth.Api.Repositories;
using RelPro.Auth.Api.Services;
using RelPro.Contracts.Dtos;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Session;

namespace RelPro.Auth.Tests.Services;

public sealed class LoginServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IContractStatusLoader _contracts = Substitute.For<IContractStatusLoader>();
    private readonly IEntitlementLoader _entitlements = Substitute.For<IEntitlementLoader>();
    private readonly ISessionStore _sessions = Substitute.For<ISessionStore>();
    private readonly ILoginLockoutService _lockout = Substitute.For<ILoginLockoutService>();

    private LoginService BuildService() =>
        new(_users, _hasher, _contracts, _entitlements, _sessions, _lockout);

    private static UserRecord ValidUser() => new(
        Id: 42,
        OrgId: 10,
        ContractId: 5,
        Email: "user@bank.com",
        FirstName: "Jane",
        LastName: "Doe",
        UserName: null,
        PasswordHash: "hash",
        Salt: "salt",
        IsActive: true,
        IsEnabled: true);

    private static ContractStatusInfo ActiveContract(int contractId) =>
        new(contractId, ContractStatus.Active, new DateOnly(2030, 1, 1), true);

    [Fact]
    public async Task LoginAsync_ReturnsInvalidCredentials_WhenUserNotFound()
    {
        _users.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((UserRecord?)null);

        var result = await BuildService().LoginAsync("nobody@x.com", "pass");

        Assert.False(result.IsSuccess);
        Assert.Equal(LoginFailureReason.InvalidCredentials, result.Failure);
    }

    [Fact]
    public async Task LoginAsync_ReturnsInvalidCredentials_WhenPasswordInvalid()
    {
        _users.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(ValidUser());
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
               .Returns(false);

        var result = await BuildService().LoginAsync("user@bank.com", "wrong");

        Assert.False(result.IsSuccess);
        Assert.Equal(LoginFailureReason.InvalidCredentials, result.Failure);
    }

    [Fact]
    public async Task LoginAsync_ReturnsInvalidCredentials_WhenContractNotFound()
    {
        _users.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(ValidUser());
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
               .Returns(true);
        _contracts.LoadAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns((ContractStatusInfo?)null);

        var result = await BuildService().LoginAsync("user@bank.com", "pass");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_ReturnsInvalidCredentials_WhenContractInactive()
    {
        var inactiveContract = new ContractStatusInfo(5, ContractStatus.Expired, null, false);
        _users.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(ValidUser());
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
               .Returns(true);
        _contracts.LoadAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                  .Returns(inactiveContract);

        var result = await BuildService().LoginAsync("user@bank.com", "pass");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_ReturnsAccountLocked_WhenLockedOut()
    {
        _lockout.IsLockedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(true);

        var result = await BuildService().LoginAsync("user@bank.com", "pass");

        Assert.False(result.IsSuccess);
        Assert.Equal(LoginFailureReason.AccountLocked, result.Failure);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsValid()
    {
        _users.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(ValidUser());
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
               .Returns(true);
        _contracts.LoadAsync(5, Arg.Any<CancellationToken>())
                  .Returns(ActiveContract(5));
        _entitlements.LoadAsync(5, Arg.Any<CancellationToken>())
                     .Returns(new ContractEntitlementsDto { ContractId = 5 });
        _sessions.CreateAsync(Arg.Any<FullSessionData>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
                 .Returns("abc123token");

        var result = await BuildService().LoginAsync("user@bank.com", "correct");

        Assert.True(result.IsSuccess);
        Assert.Equal("abc123token", result.Token);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ClearsLockout_OnSuccess()
    {
        _users.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(ValidUser());
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
               .Returns(true);
        _contracts.LoadAsync(5, Arg.Any<CancellationToken>())
                  .Returns(ActiveContract(5));
        _entitlements.LoadAsync(5, Arg.Any<CancellationToken>())
                     .Returns(new ContractEntitlementsDto { ContractId = 5 });
        _sessions.CreateAsync(Arg.Any<FullSessionData>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
                 .Returns("tok");

        await BuildService().LoginAsync("user@bank.com", "correct");

        await _lockout.Received(1).ClearAsync("user@bank.com", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_StoresCorrectUserDataInSession()
    {
        var user = ValidUser();
        _users.FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
               .Returns(true);
        _contracts.LoadAsync(5, Arg.Any<CancellationToken>())
                  .Returns(ActiveContract(5));
        _entitlements.LoadAsync(5, Arg.Any<CancellationToken>())
                     .Returns(new ContractEntitlementsDto { ContractId = 5 });
        _sessions.CreateAsync(Arg.Any<FullSessionData>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
                 .Returns("tok");

        await BuildService().LoginAsync("user@bank.com", "correct");

        await _sessions.Received(1).CreateAsync(
            Arg.Is<FullSessionData>(d =>
                d.UserId == 42 &&
                d.OrgId == 10 &&
                d.ContractId == 5 &&
                d.Email == "user@bank.com"),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }
}
