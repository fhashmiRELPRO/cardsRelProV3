using MongoDB.Driver;

namespace RelPro.Infrastructure.Database;

public interface IMongoClientFactory
{
    IMongoDatabase GetDatabase(string databaseName);
}
