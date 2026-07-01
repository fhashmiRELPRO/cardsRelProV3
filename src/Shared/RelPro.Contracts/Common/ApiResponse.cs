namespace RelPro.Contracts.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string errorCode, string message) =>
        new() { Success = false, ErrorCode = errorCode, ErrorMessage = message };
}

public sealed class ApiResponse
{
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResponse Ok() => new() { Success = true };

    public static ApiResponse Fail(string errorCode, string message) =>
        new() { Success = false, ErrorCode = errorCode, ErrorMessage = message };
}
