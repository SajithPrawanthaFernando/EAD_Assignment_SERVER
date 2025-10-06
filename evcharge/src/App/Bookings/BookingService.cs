using Domain.Bookings;
using Infra.Bookings;
using Infra.Schedules;
using Infra.Stations;

namespace App.Bookings;

public interface IBookingService
{
    Task<string> CreateAsync(BookingCreateDto dto, string? requesterNic, bool isBackoffice);
    Task UpdateAsync(BookingUpdateDto dto, string? requesterNic, bool isBackoffice);
    Task CancelAsync(string id, string? requesterNic, bool isBackoffice);
    Task<List<BookingView>> GetMineAsync(string ownerNic);
    Task<BookingView?> GetByIdAsync(string id);
    Task<List<BookingView>> GetAllAsync();
}

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly IScheduleRepository _schedules;
    private readonly IStationRepository _stations;          

    public BookingService(
        IBookingRepository bookings,
        IScheduleRepository schedules,
        IStationRepository stations)                          
    {
        _bookings = bookings;
        _schedules = schedules;
        _stations = stations;                                 
    }

    private static bool Within7Days(DateTime startUtc)
        => startUtc >= DateTime.UtcNow && startUtc <= DateTime.UtcNow.AddDays(7);

    private static bool IsTooLate(DateTime startUtc)
        => DateTime.UtcNow > startUtc.AddHours(-12);

    private static void EnsureSelfOrBackoffice(string? requesterNic, string ownerNic, bool isBackoffice)
    {
        if (isBackoffice) return;
        if (string.IsNullOrWhiteSpace(requesterNic) || !string.Equals(requesterNic, ownerNic, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("You can only manage your own bookings.");
    }

    public async Task<string> CreateAsync(BookingCreateDto dto, string? requesterNic, bool isBackoffice)
    {
        if (!Within7Days(dto.StartTimeUtc))
            throw new InvalidOperationException("Bookings must be within 7 days from now.");

        EnsureSelfOrBackoffice(requesterNic, dto.OwnerNic, isBackoffice);

        var available = await _schedules.IsAvailableAsync(dto.StationId, dto.SlotId, dto.StartTimeUtc);
        if (!available) throw new InvalidOperationException("Selected slot is not available.");

        var id = Guid.NewGuid().ToString("N");
        var booking = new Booking
        {
            Id = id,
            OwnerNic = dto.OwnerNic,
            StationId = dto.StationId,
            SlotId = dto.SlotId,
            StartTimeUtc = dto.StartTimeUtc,
            Status = BookingStatus.Pending
        };

        await _bookings.InsertAsync(booking);

        // lock the schedule and mark slot unavailable
        await _schedules.SetAvailabilityAsync(dto.StationId, dto.SlotId, dto.StartTimeUtc, false);
        await _stations.SetSlotAvailabilityAsync(dto.StationId, dto.SlotId, false); 

        return id;
    }

    public async Task UpdateAsync(BookingUpdateDto dto, string? requesterNic, bool isBackoffice)
    {
        var existing = await _bookings.GetAsync(dto.Id) ?? throw new InvalidOperationException("Booking not found.");
        EnsureSelfOrBackoffice(requesterNic, existing.OwnerNic, isBackoffice);

        if (IsTooLate(existing.StartTimeUtc))
            throw new InvalidOperationException("Cannot modify within 12 hours of start.");

        if (!Within7Days(dto.StartTimeUtc))
            throw new InvalidOperationException("Updated time must be within 7 days from now.");

        // Determine if target slot/time actually changes
        var isSameStation = string.Equals(existing.StationId, dto.StationId, StringComparison.Ordinal);
        var isSameSlot    = string.Equals(existing.SlotId, dto.SlotId, StringComparison.Ordinal);
        var isSameTime    = existing.StartTimeUtc == dto.StartTimeUtc;
        var changesTarget = !(isSameStation && isSameSlot && isSameTime);

        if (changesTarget)
        {
            // Free previous slot/time + mark slot available
            await _schedules.SetAvailabilityAsync(existing.StationId, existing.SlotId, existing.StartTimeUtc, true);
            await _stations.SetSlotAvailabilityAsync(existing.StationId, existing.SlotId, true); 

            // Check new slot/time availability
            var available = await _schedules.IsAvailableAsync(dto.StationId, dto.SlotId, dto.StartTimeUtc);
            if (!available) throw new InvalidOperationException("Selected slot is not available.");
        }

        // Update booking core fields
        existing.StationId    = dto.StationId;
        existing.SlotId       = dto.SlotId;
        existing.StartTimeUtc = dto.StartTimeUtc;

        await _bookings.UpdateCoreAsync(existing);

        if (changesTarget)
        {
            // Lock new slot/time + mark slot unavailable
            await _schedules.SetAvailabilityAsync(dto.StationId, dto.SlotId, dto.StartTimeUtc, false);
            await _stations.SetSlotAvailabilityAsync(dto.StationId, dto.SlotId, false); 
        }
    }

    public async Task CancelAsync(string id, string? requesterNic, bool isBackoffice)
    {
        var b = await _bookings.GetAsync(id) ?? throw new InvalidOperationException("Booking not found.");
        EnsureSelfOrBackoffice(requesterNic, b.OwnerNic, isBackoffice);

        if (IsTooLate(b.StartTimeUtc))
            throw new InvalidOperationException("Cannot cancel within 12 hours of start.");

        await _bookings.UpdateStatusAsync(id, BookingStatus.Cancelled);

        // free schedule + mark slot available
        await _schedules.SetAvailabilityAsync(b.StationId, b.SlotId, b.StartTimeUtc, true);
        await _stations.SetSlotAvailabilityAsync(b.StationId, b.SlotId, true); 
    }

    public async Task<List<BookingView>> GetMineAsync(string ownerNic)
    {
        var list = await _bookings.GetByOwnerAsync(ownerNic);
        return list.Select(b => new BookingView(b.Id, b.OwnerNic, b.StationId, b.SlotId, b.StartTimeUtc, b.Status.ToString())).ToList();
    }

    public async Task<BookingView?> GetByIdAsync(string id)
    {
        var b = await _bookings.GetAsync(id);
        return b is null ? null : new BookingView(b.Id, b.OwnerNic, b.StationId, b.SlotId, b.StartTimeUtc, b.Status.ToString());
    }

    public async Task<List<BookingView>> GetAllAsync()
    {
        var list = await _bookings.GetAllAsync();
        return list
            .Select(b => new BookingView(b.Id, b.OwnerNic, b.StationId, b.SlotId, b.StartTimeUtc, b.Status.ToString()))
            .ToList();
    }
}



public static class BookingRepositoryExtensions
{
    public static Task UpdateCoreAsync(this IBookingRepository repo, Booking b)
        => repo.ReplaceCoreAsync(b);
}
