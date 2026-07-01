using RelPro.Contracts.Dtos;
using RelPro.Contracts.Enums;

namespace RelPro.Contracts.Tests.Dtos;

public class ContractEntitlementsDtoTests
{
    [Fact]
    public void IsFeatureEnabled_WhenFeatureIsTrue_ReturnsTrue()
    {
        var dto = new ContractEntitlementsDto { SearchPeople = true };

        Assert.True(dto.IsFeatureEnabled(EntitlementFeature.SearchPeople));
    }

    [Fact]
    public void IsFeatureEnabled_WhenFeatureIsFalse_ReturnsFalse()
    {
        var dto = new ContractEntitlementsDto { SearchPeople = false };

        Assert.False(dto.IsFeatureEnabled(EntitlementFeature.SearchPeople));
    }

    [Fact]
    public void IsFeatureEnabled_UnknownFeatureValue_ReturnsFalse()
    {
        var dto = new ContractEntitlementsDto();

        Assert.False(dto.IsFeatureEnabled((EntitlementFeature)9999));
    }

    [Theory]
    [InlineData(EntitlementFeature.ExportPeople, nameof(ContractEntitlementsDto.ExportPeople))]
    [InlineData(EntitlementFeature.CRMSalesforce, nameof(ContractEntitlementsDto.CRMSalesforce))]
    [InlineData(EntitlementFeature.DataDirectDial, nameof(ContractEntitlementsDto.DataDirectDial))]
    [InlineData(EntitlementFeature.ReportingAdvanced, nameof(ContractEntitlementsDto.ReportingAdvanced))]
    [InlineData(EntitlementFeature.AIInsights, nameof(ContractEntitlementsDto.AIInsights))]
    public void IsFeatureEnabled_MapsFeatureEnumToCorrectProperty(EntitlementFeature feature, string propertyName)
    {
        var dto = BuildDtoWithFeatureEnabled(propertyName);

        Assert.True(dto.IsFeatureEnabled(feature));
    }

    private static ContractEntitlementsDto BuildDtoWithFeatureEnabled(string propertyName)
    {
        var prop = typeof(ContractEntitlementsDto).GetProperty(propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on ContractEntitlementsDto");

        var props = typeof(ContractEntitlementsDto)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(bool))
            .ToDictionary(p => p.Name, _ => false);

        props[propertyName] = true;

        return BuildFromDict(props);
    }

    private static ContractEntitlementsDto BuildFromDict(Dictionary<string, bool> values)
    {
        var type = typeof(ContractEntitlementsDto);
        var instance = Activator.CreateInstance(type)!;

        foreach (var (name, value) in values)
        {
            var prop = type.GetProperty(name);
            if (prop?.CanWrite == true)
                prop.SetValue(instance, value);
        }

        return (ContractEntitlementsDto)instance;
    }
}
