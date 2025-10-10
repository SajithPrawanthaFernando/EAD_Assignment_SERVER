// ScheduleRepository.cs
using Domain.Schedules;
using MongoDB.Driver;
using Infra.Mongo;

namespace Infra.Schedules;

public interface IScheduleRepository
{
    Task<bool> IsAvailableAsync(string stationId, string slotId, DateTime startUtc);
    Task SetAvailabilityAsync(string stationId, string slotId, DateTime startUtc, bool isAvailable);
}

public sealed class ScheduleRepository : IScheduleRepository
{
    private readonly IMongoCollection<Schedule> _col;
    public ScheduleRepository(IMongoContext ctx) => _col = ctx.GetCollection<Schedule>("schedules");

    public async Task<bool> IsAvailableAsync(string stationId, string slotId, DateTime startUtc)
    {
        var s = await _col.Find(x => x.StationId == stationId && x.SlotId == slotId && x.StartTimeUtc == startUtc)
                          .FirstOrDefaultAsync();
        return s?.IsAvailable ?? true;
    }

    public async Task SetAvailabilityAsync(string stationId, string slotId, DateTime startUtc, bool isAvailable)
    {
        var filter = Builders<Schedule>.Filter.Where(x =>
            x.StationId == stationId && x.SlotId == slotId && x.StartTimeUtc == startUtc);

        var update = Builders<Schedule>.Update
            .SetOnInsert(x => x.StationId, stationId)
            .SetOnInsert(x => x.SlotId, slotId)
            .SetOnInsert(x => x.StartTimeUtc, startUtc)
            .Set(x => x.IsAvailable, isAvailable);

        await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }
}
