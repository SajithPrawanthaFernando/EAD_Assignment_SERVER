using Domain.Bookings;
using MongoDB.Driver;
using Infra.Mongo;

namespace Infra.Bookings;

public interface IBookingRepository
{
    Task InsertAsync(Booking b);
    Task<Booking?> GetAsync(string id);
    Task UpdateStatusAsync(string id, BookingStatus status);
    Task<bool> HasFutureBookingsForStationAsync(string stationId, DateTime nowUtc);
}

public sealed class BookingRepository : IBookingRepository
{
    private readonly IMongoCollection<Booking> _col;
    public BookingRepository(IMongoContext ctx) => _col = ctx.GetCollection<Booking>("bookings");

    public Task InsertAsync(Booking b) => _col.InsertOneAsync(b);

    public async Task<Booking?> GetAsync(string id) =>
        await _col.Find(x => x.Id == id).FirstOrDefaultAsync();

    public Task UpdateStatusAsync(string id, BookingStatus status) =>
        _col.UpdateOneAsync(x => x.Id == id, Builders<Booking>.Update.Set(x => x.Status, status));

    public async Task<bool> HasFutureBookingsForStationAsync(string stationId, DateTime nowUtc)
    {
        var filter = Builders<Booking>.Filter.Where(x =>
             x.StationId == stationId &&
             (x.Status == BookingStatus.Pending || x.Status == BookingStatus.Approved) &&
             x.StartTimeUtc >= nowUtc);
        return await _col.Find(filter).AnyAsync();
    }
}
