namespace RelPro.Contracts.Common;

public sealed class PaginationRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int Offset => (Page - 1) * PageSize;

    public PaginationRequest Clamp(int maxPageSize = 100) =>
        new() { Page = Math.Max(1, Page), PageSize = Math.Clamp(PageSize, 1, maxPageSize) };
}
