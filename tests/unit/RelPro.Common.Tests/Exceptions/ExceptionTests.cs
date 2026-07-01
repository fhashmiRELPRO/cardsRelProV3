using RelPro.Common.Exceptions;

namespace RelPro.Common.Tests.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void ResourceNotFoundException_MessageContainsTypeAndId()
    {
        var ex = new ResourceNotFoundException("User", 42);

        Assert.Contains("User", ex.Message);
        Assert.Contains("42", ex.Message);
        Assert.Equal("User", ex.ResourceType);
        Assert.Equal(42, ex.ResourceId);
    }

    [Fact]
    public void EntitlementException_MessageContainsFeatureName()
    {
        var ex = new EntitlementException("BulkExport");

        Assert.Contains("BulkExport", ex.Message);
        Assert.Equal("BulkExport", ex.RequiredFeature);
    }

    [Fact]
    public void ContractInactiveException_MessageContainsContractId()
    {
        var ex = new ContractInactiveException(99);

        Assert.Contains("99", ex.Message);
        Assert.Equal(99, ex.ContractId);
    }

    [Fact]
    public void AllExceptions_InheritFromException()
    {
        Assert.IsAssignableFrom<Exception>(new ResourceNotFoundException("X", 1));
        Assert.IsAssignableFrom<Exception>(new EntitlementException("X"));
        Assert.IsAssignableFrom<Exception>(new ContractInactiveException(1));
    }
}
