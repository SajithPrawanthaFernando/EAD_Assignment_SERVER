// src/Infra/Users/UserRepository.cs
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
}
