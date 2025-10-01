using Domain.Users;
using MongoDB.Driver;
using Infra.Mongo;

namespace Infra.Users;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task InsertAsync(User u);
    Task<long> CountAsync();
    IMongoCollection<User> Collection { get; }
}

public sealed class UserRepository : IUserRepository
{
    public IMongoCollection<User> Collection { get; }
    public UserRepository(IMongoContext ctx)
    {
        Collection = ctx.GetCollection<User>("users");
    }

    public async Task<User?> FindByEmailAsync(string email) =>
        await Collection.Find(x => x.Email == email).FirstOrDefaultAsync();

    public Task InsertAsync(User u) => Collection.InsertOneAsync(u);

    public async Task<long> CountAsync() => await Collection.CountDocumentsAsync(_ => true);
}
