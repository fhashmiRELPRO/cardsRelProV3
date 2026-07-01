namespace RelPro.Common.Exceptions;

public sealed class ServiceUnavailableException : Exception
{
    public string ServiceName { get; }

    public ServiceUnavailableException(string serviceName, Exception? inner = null)
        : base($"The '{serviceName}' service is temporarily unavailable. Please try again later.", inner)
    {
        ServiceName = serviceName;
    }
}
