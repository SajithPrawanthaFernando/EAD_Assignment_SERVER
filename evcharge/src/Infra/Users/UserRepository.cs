using Domain.Users;
using MongoDB.Bson;
using MongoDB.Driver;
using Infra.Mongo;

namespace Infra.Users;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task InsertAsync(User u);
    Task<long> CountAsync();

    Task UpsertSeedUserAsync(string email, string passwordHash, Role[] roles);

    IMongoCollection<User> Collection { get; }

    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> FindByOwnerNicAsync(string nic);

    Task DeleteByOwnerNicAsync(string ownerNic);
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

    public async Task<List<User>> GetAllAsync()
    {
        return await Collection.Find(_ => true)
            .SortBy(x => x.Email)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string id) =>
        await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<User?> FindByOwnerNicAsync(string nic) =>
        await Collection.Find(x => x.OwnerNic == nic).FirstOrDefaultAsync();

    public Task UpsertSeedUserAsync(string email, string passwordHash, Role[] roles)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Email, email);

        var update = Builders<User>.Update

            .SetOnInsert(x => x.Id, ObjectId.GenerateNewId().ToString())
            .SetOnInsert(x => x.Email, email)
            .SetOnInsert(x => x.PasswordHash, passwordHash)
            .SetOnInsert(x => x.Roles, roles)
            .SetOnInsert(x => x.Active, true);

        return Collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public Task DeleteByOwnerNicAsync(string ownerNic) =>
        Collection.DeleteOneAsync(x => x.OwnerNic == ownerNic);
    
    
}
