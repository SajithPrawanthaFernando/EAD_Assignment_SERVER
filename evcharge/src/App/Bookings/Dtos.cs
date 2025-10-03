namespace App.Bookings;

public sealed record BookingCreateDto(string OwnerNic, string StationId, string SlotId, DateTime StartTimeUtc);
public sealed record BookingUpdateDto(string Id, string StationId, string SlotId, DateTime StartTimeUtc);
public sealed record BookingView(string Id, string OwnerNic, string StationId, string SlotId, DateTime StartTimeUtc, string Status);
