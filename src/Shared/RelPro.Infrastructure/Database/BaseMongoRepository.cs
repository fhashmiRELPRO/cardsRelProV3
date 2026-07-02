using MongoDB.Driver;
using RelPro.Contracts.Common;

namespace RelPro.Infrastructure.Database;

public abstract class BaseMongoRepository<T> where T : class
{
    private readonly IMongoClientFactory _factory;

    protected abstract string DatabaseName { get; }
    protected abstract string CollectionName { get; }

    protected BaseMongoRepository(IMongoClientFactory factory) => _factory = factory;

    protected IMongoCollection<T> Collection =>
        _factory.GetDatabase(DatabaseName).GetCollection<T>(CollectionName);

    protected async Task<T?> FindOneAsync(
        FilterDefinition<T> filter, CancellationToken ct = default) =>
        await Collection.Find(filter).FirstOrDefaultAsync(ct);

    protected async Task<PagedResult<T>> FindPagedAsync(
        FilterDefinition<T> filter,
        SortDefinition<T>? sort = null,
        int page = 1, int pageSize = 20,
        CancellationToken ct = default)
    {
        var totalCount = (int)await Collection.CountDocumentsAsync(filter, cancellationToken: ct);
        if (totalCount == 0)
            return PagedResult<T>.Empty(page, pageSize);

        var query = Collection.Find(filter);
        if (sort is not null) query = query.Sort(sort);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    protected async Task InsertAsync(T document, CancellationToken ct = default) =>
        await Collection.InsertOneAsync(document, cancellationToken: ct);

    protected async Task InsertManyAsync(IEnumerable<T> documents, CancellationToken ct = default) =>
        await Collection.InsertManyAsync(documents, cancellationToken: ct);

    protected async Task<bool> UpsertAsync(
        FilterDefinition<T> filter, T document, CancellationToken ct = default)
    {
        var result = await Collection.ReplaceOneAsync(
            filter, document,
            new ReplaceOptions { IsUpsert = true },
            ct);
        return result.IsAcknowledged;
    }

    protected async Task<bool> UpdateAsync(
        FilterDefinition<T> filter, UpdateDefinition<T> update, CancellationToken ct = default)
    {
        var result = await Collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    protected async Task<bool> DeleteAsync(FilterDefinition<T> filter, CancellationToken ct = default)
    {
        var result = await Collection.DeleteOneAsync(filter, ct);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    protected async Task<long> CountAsync(
        FilterDefinition<T> filter, CancellationToken ct = default) =>
        await Collection.CountDocumentsAsync(filter, cancellationToken: ct);
}
