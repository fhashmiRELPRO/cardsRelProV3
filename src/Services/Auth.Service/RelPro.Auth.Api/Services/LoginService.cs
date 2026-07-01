using RelPro.Auth.Api.Models;
using RelPro.Auth.Api.Repositories;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Session;

namespace RelPro.Auth.Api.Services;

public sealed class LoginService : ILoginService
{
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(8);

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IContractStatusLoader _contracts;
    private readonly IEntitlementLoader _entitlements;
    private readonly ISessionStore _sessions;
    private readonly ILoginLockoutService _lockout;

    public LoginService(
        IUserRepository users,
        IPasswordHasher hasher,
        IContractStatusLoader contracts,
        IEntitlementLoader entitlements,
        ISessionStore sessions,
        ILoginLockoutService lockout)
    {
        _users     = users;
        _hasher    = hasher;
        _contracts = contracts;
        _entitlements = entitlements;
        _sessions  = sessions;
        _lockout   = lockout;
    }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        if (await _lockout.IsLockedAsync(normalized, ct))
            return LoginResult.AccountLocked();

        var user = await _users.FindByEmailAsync(normalized, ct);
        if (user is null)
        {
            await _lockout.RecordFailureAsync(normalized, ct);
            return LoginResult.InvalidCredentials();
        }

        if (!user.IsActive || !user.IsEnabled)
            return LoginResult.InvalidCredentials();

        if (!_hasher.Verify(normalized, password, user.PasswordHash, user.Salt))
        {
            await _lockout.RecordFailureAsync(normalized, ct);
            return LoginResult.InvalidCredentials();
        }

        var contract = await _contracts.LoadAsync(user.ContractId, ct);
        if (contract is null || !contract.IsActive)
            return LoginResult.InvalidCredentials();

        await _lockout.ClearAsync(normalized, ct);

        var entitlements = await _entitlements.LoadAsync(user.ContractId, ct);

        var sessionData = new FullSessionData(
            UserId: user.Id,
            OrgId: user.OrgId,
            ContractId: user.ContractId,
            Email: user.Email,
            UserName: user.DisplayName,
            DataSourceId: 0,
            ValidatedAt: DateTime.UtcNow,
            ContractStatus: contract.Status,
            ContractExpiry: contract.ExpiryDate,
            IsContractActive: contract.IsActive,
            Entitlements: entitlements);

        var token = await _sessions.CreateAsync(sessionData, SessionTtl, ct);

        return LoginResult.Success(token, DateTime.UtcNow.Add(SessionTtl));
    }
}
