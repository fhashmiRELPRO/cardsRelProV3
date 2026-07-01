using RelPro.Contracts.Common;

namespace RelPro.Common.Extensions;

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) =>
        source is null || !source.Any();

    public static PagedResult<T> ToPagedResult<T>(
        this IEnumerable<T> source, int page, int pageSize, int totalCount) =>
        new() { Items = source.ToList(), Page = page, PageSize = pageSize, TotalCount = totalCount };

    public static PagedResult<TOut> ToPagedResult<TIn, TOut>(
        this IEnumerable<TIn> source,
        Func<TIn, TOut> map,
        int page, int pageSize, int totalCount) =>
        new() { Items = source.Select(map).ToList(), Page = page, PageSize = pageSize, TotalCount = totalCount };
}
