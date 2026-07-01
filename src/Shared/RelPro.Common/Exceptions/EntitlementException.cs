namespace RelPro.Common.Exceptions;

public sealed class EntitlementException : Exception
{
    public string RequiredFeature { get; }

    public EntitlementException(string requiredFeature)
        : base($"Access denied. Contract does not include entitlement: {requiredFeature}.")
    {
        RequiredFeature = requiredFeature;
    }
}
