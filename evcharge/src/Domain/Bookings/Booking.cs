// Booking.cs
namespace Domain.Bookings;

public sealed class Booking
{
    public string Id { get; set; } = default!;
    public string OwnerNic { get; set; } = default!;
    public string StationId { get; set; } = default!;
    public string SlotId { get; set; } = default!;
    public DateTime StartTimeUtc { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}
