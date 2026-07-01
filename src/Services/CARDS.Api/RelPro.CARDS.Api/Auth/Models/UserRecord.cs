namespace RelPro.CARDS.Api.Auth.Models;

public sealed record UserRecord(
    int Id,
    int OrgId,
    int ContractId,
    string Email,
    string FirstName,
    string LastName,
    string? UserName,
    string PasswordHash,
    string? Salt,
    bool IsActive,
    bool IsEnabled)
{
    public string DisplayName =>
        string.IsNullOrWhiteSpace(UserName)
            ? $"{FirstName} {LastName}".Trim()
            : UserName;
}
