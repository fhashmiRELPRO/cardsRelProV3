namespace RelPro.Common.Exceptions;

public sealed class QuotaExceededException : Exception
{
    public string QuotaType { get; }

    public QuotaExceededException(string quotaType)
        : base($"Quota exceeded for '{quotaType}'. Please upgrade your plan or try again later.")
    {
        QuotaType = quotaType;
    }
}
