using RelPro.Contracts.Dtos;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Context;

namespace RelPro.Infrastructure.Testing;

public sealed class FakeRequestContext : IRequestContext
{
    public bool IsPopulated { get; init; } = true;
    public int UserId { get; init; } = 1001;
    public int OrgId { get; init; } = 10;
    public int ContractId { get; init; } = 500;
    public string UserEmail { get; init; } = "test@relpro.com";
    public string UserName { get; init; } = "Test User";
    public ContractStatus ContractStatus { get; init; } = ContractStatus.Active;
    public DateOnly? ContractExpiry { get; init; } = new DateOnly(2030, 1, 1);
    public bool IsContractActive { get; init; } = true;
    public ContractEntitlementsDto Entitlements { get; init; } = AllEntitlements();
    public int DataSourceId { get; init; } = 1;
    public string SessionToken { get; init; } = "test-token-abc123";
    public DateTime SessionValidatedAt { get; init; } = DateTime.UtcNow;

    public bool HasEntitlement(EntitlementFeature feature) => Entitlements.IsFeatureEnabled(feature);

    public static FakeRequestContext Default => new();

    public static FakeRequestContext NoEntitlements => new()
    {
        Entitlements = new ContractEntitlementsDto { ContractId = 500 }
    };

    public static FakeRequestContext WithEntitlement(EntitlementFeature feature)
    {
        var props = typeof(ContractEntitlementsDto)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(bool));

        var dto = new ContractEntitlementsDto { ContractId = 500 };
        var type = typeof(ContractEntitlementsDto);
        var instance = Activator.CreateInstance(type)!;

        var featureName = feature.ToString();
        foreach (var prop in props)
        {
            if (prop.CanWrite)
                prop.SetValue(instance, prop.Name == featureName);
        }

        return new FakeRequestContext { Entitlements = (ContractEntitlementsDto)instance };
    }

    private static ContractEntitlementsDto AllEntitlements()
    {
        var type = typeof(ContractEntitlementsDto);
        var instance = Activator.CreateInstance(type)!;
        foreach (var prop in type.GetProperties().Where(p => p.PropertyType == typeof(bool) && p.CanWrite))
            prop.SetValue(instance, true);
        return (ContractEntitlementsDto)instance;
    }
}
