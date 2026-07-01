namespace RelPro.Infrastructure.Session;

public sealed record SessionValidationResult(
    int UserId,
    int OrgId,
    int ContractId,
    string Email,
    string UserName,
    int DataSourceId,
    DateTime ValidatedAt
);
