using Domain.Bookings;
using Infra.Bookings;
using Infra.Schedules;

namespace App.Bookings;

public interface IBookingService
{
    Task<string> CreateAsync(BookingCreateDto dto, string? requesterNic, bool isBackoffice);
    Task UpdateAsync(BookingUpdateDto dto, string? requesterNic, bool isBackoffice);
    Task CancelAsync(string id, string? requesterNic, bool isBackoffice);
    Task<List<BookingView>> GetMineAsync(string ownerNic);
    Task<BookingView?> GetByIdAsync(string id);
}

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly IScheduleRepository _schedules;

    public BookingService(IBookingRepository bookings, IScheduleRepository schedules)
    {
        _bookings = bookings; _schedules = schedules;
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
        // lock the schedule immediately
        await _schedules.SetAvailabilityAsync(dto.StationId, dto.SlotId, dto.StartTimeUtc, false);
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

        // Free previous slot time
        await _schedules.SetAvailabilityAsync(existing.StationId, existing.SlotId, existing.StartTimeUtc, true);

        // Check new slot time availability
        var available = await _schedules.IsAvailableAsync(dto.StationId, dto.SlotId, dto.StartTimeUtc);
        if (!available) throw new InvalidOperationException("Selected slot is not available.");

        // Update booking core fields (simple repo method for status change already exists; here we just replace via small helper)
        existing.StationId = dto.StationId;
        existing.SlotId = dto.SlotId;
        existing.StartTimeUtc = dto.StartTimeUtc;
        // keep status as-is (Pending/Approved etc.)

        // Minimal replace via repository (add this helper or reuse your repo Update method if you have one)
        await _bookings.UpdateCoreAsync(existing); // <-- see extension below

        // Lock new slot time
        await _schedules.SetAvailabilityAsync(dto.StationId, dto.SlotId, dto.StartTimeUtc, false);
    }

    public async Task CancelAsync(string id, string? requesterNic, bool isBackoffice)
    {
        var b = await _bookings.GetAsync(id) ?? throw new InvalidOperationException("Booking not found.");
        EnsureSelfOrBackoffice(requesterNic, b.OwnerNic, isBackoffice);

        if (IsTooLate(b.StartTimeUtc))
            throw new InvalidOperationException("Cannot cancel within 12 hours of start.");

        await _bookings.UpdateStatusAsync(id, BookingStatus.Cancelled);
        await _schedules.SetAvailabilityAsync(b.StationId, b.SlotId, b.StartTimeUtc, true);
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
}

// Small repo extension contract; add these to your IBookingRepository and implementation
public static class BookingRepositoryExtensions
{
    public static Task UpdateCoreAsync(this IBookingRepository repo, Booking b)
        => repo.ReplaceCoreAsync(b);
}
