using App.Stations;

namespace App.Bookings;

public sealed record BookingWithStationView(
    string Id,
    string OwnerNic,
    string SlotId,
    DateTime StartTimeUtc,
    string Status,
    StationView Station    
);