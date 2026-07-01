using RelPro.Contracts.Dtos;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Context;
using RelPro.Infrastructure.Entitlements;
using RelPro.Infrastructure.Session;

namespace RelPro.Infrastructure.Tests.Context;

public class RequestContextHolderTests
{
    private readonly SessionValidationResult _session = new(1001, 10, 500, "test@relpro.com", "Test User", 1, DateTime.UtcNow);
    private readonly ContractStatusInfo _contract = new(500, ContractStatus.Active, null, true);
    private readonly ContractEntitlementsDto _entitlements = new() { SearchPeople = true };

    [Fact]
    public void IsPopulated_BeforePopulate_IsFalse()
    {
        var holder = new RequestContextHolder();

        Assert.False(holder.IsPopulated);
    }

    [Fact]
    public void IsPopulated_AfterPopulate_IsTrue()
    {
        var holder = new RequestContextHolder();
        holder.Populate("token", _session, _contract, _entitlements);

        Assert.True(holder.IsPopulated);
    }

    [Fact]
    public void UserId_AfterPopulate_ReturnsSessionUserId()
    {
        var holder = new RequestContextHolder();
        holder.Populate("token", _session, _contract, _entitlements);

        Assert.Equal(1001, holder.UserId);
    }

    [Fact]
    public void IsContractActive_WhenContractActive_ReturnsTrue()
    {
        var holder = new RequestContextHolder();
        holder.Populate("token", _session, _contract, _entitlements);

        Assert.True(holder.IsContractActive);
    }

    [Fact]
    public void HasEntitlement_WhenEntitlementEnabled_ReturnsTrue()
    {
        var holder = new RequestContextHolder();
        holder.Populate("token", _session, _contract, _entitlements);

        Assert.True(holder.HasEntitlement(EntitlementFeature.SearchPeople));
    }

    [Fact]
    public void HasEntitlement_WhenEntitlementNotEnabled_ReturnsFalse()
    {
        var holder = new RequestContextHolder();
        holder.Populate("token", _session, _contract, _entitlements);

        Assert.False(holder.HasEntitlement(EntitlementFeature.ExportBulk));
    }

    [Fact]
    public void UserId_BeforePopulate_ThrowsInvalidOperationException()
    {
        var holder = new RequestContextHolder();

        Assert.Throws<InvalidOperationException>(() => holder.UserId);
    }

    [Fact]
    public void SessionToken_AfterPopulate_ReturnsToken()
    {
        var holder = new RequestContextHolder();
        holder.Populate("my-token", _session, _contract, _entitlements);

        Assert.Equal("my-token", holder.SessionToken);
    }
}
