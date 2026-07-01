using RelPro.Contracts.Enums;

namespace RelPro.Infrastructure.Entitlements;

public sealed record ContractStatusInfo(
    int ContractId,
    ContractStatus Status,
    DateOnly? ExpiryDate,
    bool IsActive
);
