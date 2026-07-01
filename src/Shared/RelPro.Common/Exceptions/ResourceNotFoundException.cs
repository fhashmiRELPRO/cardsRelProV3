namespace RelPro.Common.Exceptions;

public sealed class ResourceNotFoundException : Exception
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public ResourceNotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with id '{resourceId}' was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
