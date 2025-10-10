// EvOwnerRepository.cs
using Domain.EvOwners;
using MongoDB.Driver;
using Infra.Mongo;
using MongoDB.Bson;

namespace Infra.EvOwners;

public interface IEvOwnerRepository
{
    Task<EvOwner?> GetAsync(string nic);
    Task UpsertAsync(EvOwner owner);
    Task<bool> ExistsAsync(string nic);
    Task SetStatusAsync(string nic, EvOwnerStatus status);

    Task<List<EvOwner>> GetAllAsync();
     
    Task DeleteAsync(string nic);
}

public sealed class EvOwnerRepository : IEvOwnerRepository
{
    private readonly IMongoCollection<EvOwner> _col;
    public EvOwnerRepository(IMongoContext ctx) => _col = ctx.GetCollection<EvOwner>("ev_owners");

    public async Task<EvOwner?> GetAsync(string nic) =>
        await _col.Find(x => x.Nic == nic).FirstOrDefaultAsync();

    public async Task UpsertAsync(EvOwner o)
    {
        var filter = Builders<EvOwner>.Filter.Eq(x => x.Nic, o.Nic);

        var update = Builders<EvOwner>.Update
            // set on both insert & update
            .Set(x => x.Name, o.Name)
            .Set(x => x.Phone, o.Phone)
            .Set(x => x.Status, o.Status)
            // ensure keys on insert
            .SetOnInsert(x => x.Nic, o.Nic)
            .SetOnInsert(x => x.Id, ObjectId.GenerateNewId().ToString());

        await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task<bool> ExistsAsync(string nic) =>
        await _col.Find(x => x.Nic == nic).AnyAsync();

    public Task SetStatusAsync(string nic, EvOwnerStatus status) =>
        _col.UpdateOneAsync(x => x.Nic == nic, Builders<EvOwner>.Update.Set(x => x.Status, status));

    public async Task<List<EvOwner>> GetAllAsync() =>
        await _col.Find(_ => true).SortBy(x => x.Nic).ToListAsync();
        
    public Task DeleteAsync(string nic) =>
        _col.DeleteOneAsync(x => x.Nic == nic);
}
