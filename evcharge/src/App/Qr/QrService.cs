using System.Security.Cryptography;
using Domain.Bookings;
using Domain.Qr;
using Infra.Bookings;
using Infra.Qr;
using Infra.Schedules;
using Infra.Stations;  

namespace App.Qr;

public interface IQrService
{
    Task<string> IssueForBookingAsync(string bookingId, DateTime expUtc);
    Task<(string BookingId, bool Valid)> VerifyAsync(string token);

    Task StartChargingAsync(string bookingId);

    Task CompleteAsync(string bookingId);                             
}

public sealed class QrService : IQrService
{
    private readonly IQrRepository _qr;
    private readonly IBookingRepository _bookings;
    private readonly IScheduleRepository _schedules;
    private readonly IStationRepository _stations;

    public QrService(
        IQrRepository qr,
        IBookingRepository bookings,
        IScheduleRepository schedules,
        IStationRepository stations)
    {
        _qr = qr;
        _bookings = bookings;
        _schedules = schedules;
        _stations = stations;
    }

    public async Task<string> IssueForBookingAsync(string bookingId, DateTime expUtc)
    {
        // Ensure booking exists and is valid
        var b = await _bookings.GetAsync(bookingId) ?? throw new InvalidOperationException("Booking not found.");
        if (b.Status != BookingStatus.Approved && b.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Booking not in a state to issue QR.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        await _qr.InsertAsync(new QrToken
        {
            BookingId = bookingId,
            Token = token,
            ExpUtc = expUtc
        });
        return token;
    }

    public async Task<(string BookingId, bool Valid)> VerifyAsync(string token)
    {
        var t = await _qr.FindByTokenAsync(token);
        if (t is null) return ("", false);

        if (DateTime.UtcNow > t.ExpUtc) return (t.BookingId, false);

        // Confirm booking still active
        var b = await _bookings.GetAsync(t.BookingId);
        if (b is null) return (t.BookingId, false);
        if (b.Status == BookingStatus.Cancelled) return (t.BookingId, false);

        return (t.BookingId, true);
    }
    
     public async Task StartChargingAsync(string bookingId)
    {
        var b = await _bookings.GetAsync(bookingId) ?? throw new InvalidOperationException("Booking not found.");

        if (b.Status != BookingStatus.Approved && b.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Booking must be Approved (or Pending if you allow) before starting charge.");

        
        await _bookings.UpdateStatusAsync(bookingId, BookingStatus.Charging);
    }

    public async Task CompleteAsync(string bookingId)
    {
        var b = await _bookings.GetAsync(bookingId) ?? throw new InvalidOperationException("Booking not found.");
        if (b.Status != BookingStatus.Charging)
            throw new InvalidOperationException("Only Charging bookings can be completed.");

        await _bookings.UpdateStatusAsync(bookingId, BookingStatus.Completed);

        
        await _schedules.SetAvailabilityAsync(b.StationId, b.SlotId, b.StartTimeUtc, true);
        await _stations.SetSlotAvailabilityAsync(b.StationId, b.SlotId, true);
    }
}
