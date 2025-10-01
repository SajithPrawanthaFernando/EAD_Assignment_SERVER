using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infra.Mongo;

public interface IMongoContext
{
    IMongoDatabase Database { get; }
    IMongoCollection<T> GetCollection<T>(string name);
}

public sealed class MongoContext : IMongoContext
{
    private readonly IMongoDatabase _db;
    public MongoContext(IOptions<MongoSettings> opts)
    {
        var client = new MongoClient(opts.Value.ConnectionString);
        _db = client.GetDatabase(opts.Value.Database);
    }
    public IMongoDatabase Database => _db;
    public IMongoCollection<T> GetCollection<T>(string name) => _db.GetCollection<T>(name);
}
