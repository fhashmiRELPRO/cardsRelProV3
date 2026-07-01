namespace RelPro.Auth.Api.Models;

public enum LoginFailureReason { InvalidCredentials, AccountLocked }

public sealed record LoginResult
{
    public string? Token { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public LoginFailureReason? Failure { get; init; }

    public bool IsSuccess => Token is not null;

    public static LoginResult Success(string token, DateTime expiresAt) =>
        new() { Token = token, ExpiresAt = expiresAt };

    public static LoginResult InvalidCredentials() =>
        new() { Failure = LoginFailureReason.InvalidCredentials };

    public static LoginResult AccountLocked() =>
        new() { Failure = LoginFailureReason.AccountLocked };
}
