namespace RelPro.CARDS.Api.User.Models;

public sealed record UserProfileResponse(
    int UserId,
    int OrgId,
    int ContractId,
    string Email,
    string DisplayName,
    bool IsContractActive,
    string ContractStatus,
    string? ContractExpiry,
    string? PendoAccountId = null);
