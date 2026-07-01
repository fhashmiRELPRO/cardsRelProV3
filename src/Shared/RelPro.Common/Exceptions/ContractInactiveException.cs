namespace RelPro.Common.Exceptions;

public sealed class ContractInactiveException : Exception
{
    public int ContractId { get; }

    public ContractInactiveException(int contractId)
        : base($"Contract {contractId} is inactive or expired.")
    {
        ContractId = contractId;
    }
}
