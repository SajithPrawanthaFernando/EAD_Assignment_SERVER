using Domain.Stations;
using MongoDB.Driver;
using Infra.Mongo;

namespace Infra.Stations;

public interface IStationRepository
{
    Task<Station?> GetAsync(string id);
    Task InsertAsync(Station s);
    Task UpdateAsync(Station s);
    Task<List<Station>> GetAllAsync();
    Task SetActiveAsync(string id, bool active);
}

public sealed class StationRepository : IStationRepository
{
    private readonly IMongoCollection<Station> _col;
    public StationRepository(IMongoContext ctx) => _col = ctx.GetCollection<Station>("stations");

    public Task InsertAsync(Station s) => _col.InsertOneAsync(s);
    public Task UpdateAsync(Station s) =>
        _col.ReplaceOneAsync(x => x.Id == s.Id, s);

    public async Task<Station?> GetAsync(string id) =>
        await _col.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<Station>> GetAllAsync() =>
        await _col.Find(FilterDefinition<Station>.Empty).ToListAsync();

    public Task SetActiveAsync(string id, bool active) =>
        _col.UpdateOneAsync(x => x.Id == id, Builders<Station>.Update.Set(x => x.Active, active));
}
