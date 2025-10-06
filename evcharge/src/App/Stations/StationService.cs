using Domain.Stations;
using Infra.Stations;
using Infra.Bookings;

namespace App.Stations;

public interface IStationService
{
    Task<string> CreateAsync(StationCreateDto dto);
    Task UpdateAsync(StationUpdateDto dto);
    Task<List<StationView>> GetAllAsync();
    Task SetActiveAsync(string id, bool active);
}

public sealed class StationService : IStationService
{
    private readonly IStationRepository _repo;
    private readonly IBookingRepository _bookings;

    public StationService(IStationRepository repo, IBookingRepository bookings)
    {
        _repo = repo; _bookings = bookings;
    }

    public async Task<string> CreateAsync(StationCreateDto dto)
{
    var s = new Station
    {
        Id = Guid.NewGuid().ToString("N"),
        Name = dto.Name,
        Type = dto.Type,
        Lat = dto.Lat,
        Lng = dto.Lng,
        Active = true,
        Slots = dto.Slots.Select(x => new StationSlot
        {
            SlotId = x.SlotId,
            Label = x.Label,
            Available = true 
        }).ToList()
    };
    await _repo.InsertAsync(s);
    return s.Id;
}

    public async Task UpdateAsync(StationUpdateDto dto)
{
    var s = await _repo.GetAsync(dto.Id) ?? throw new InvalidOperationException("Station not found.");
    s.Name = dto.Name;
    s.Type = dto.Type;
    s.Lat = dto.Lat;
    s.Lng = dto.Lng;
    s.Slots = dto.Slots.Select(x => new StationSlot
    {
        SlotId = x.SlotId,
        Label = x.Label,
        Available = true 
    }).ToList();
    await _repo.UpdateAsync(s);
}

   public async Task<List<StationView>> GetAllAsync()
{
    var all = await _repo.GetAllAsync();
    return all.Select(s => new StationView(
        s.Id,
        s.Name,
        s.Type.ToString(),
        s.Active,
        s.Lat,
        s.Lng,
        s.Slots.Select(x => new StationSlotDto(x.SlotId, x.Label, x.Available)).ToList()
    )).ToList();
}

    public async Task SetActiveAsync(string id, bool active)
    {
        if (!active)
        {
            var hasFuture = await _bookings.HasFutureBookingsForStationAsync(id, DateTime.UtcNow);
            if (hasFuture) throw new InvalidOperationException("Cannot deactivate: station has upcoming bookings.");
        }
        await _repo.SetActiveAsync(id, active);
    }
}
