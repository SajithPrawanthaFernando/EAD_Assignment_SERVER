using Domain.EvOwners;
using MongoDB.Driver;
using Infra.Mongo;

namespace Infra.EvOwners;

public interface IEvOwnerRepository
{
    Task<EvOwner?> GetAsync(string nic);
    Task UpsertAsync(EvOwner owner);           
    Task<bool> ExistsAsync(string nic);
    Task SetStatusAsync(string nic, EvOwnerStatus status);
}

public sealed class EvOwnerRepository : IEvOwnerRepository
{
    private readonly IMongoCollection<EvOwner> _col;
    public EvOwnerRepository(IMongoContext ctx) => _col = ctx.GetCollection<EvOwner>("ev_owners");

    public async Task<EvOwner?> GetAsync(string nic) =>
        await _col.Find(x => x.Nic == nic).FirstOrDefaultAsync();

    public async Task UpsertAsync(EvOwner o) =>
        await _col.ReplaceOneAsync(x => x.Nic == o.Nic, o, new ReplaceOptions { IsUpsert = true });

    public async Task<bool> ExistsAsync(string nic) =>
        await _col.Find(x => x.Nic == nic).AnyAsync();

    public Task SetStatusAsync(string nic, EvOwnerStatus status) =>
        _col.UpdateOneAsync(x => x.Nic == nic, Builders<EvOwner>.Update.Set(x => x.Status, status));
}
