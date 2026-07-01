using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace RelPro.Infrastructure.Database;

public sealed class MongoClientFactory : IMongoClientFactory
{
    private readonly IMongoClient _client;

    public MongoClientFactory(IOptions<MongoOptions> options)
    {
        _client = new MongoClient(options.Value.ConnectionString);
    }

    public IMongoDatabase GetDatabase(string databaseName) =>
        _client.GetDatabase(databaseName);
}

public sealed class MongoOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}
