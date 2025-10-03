using System.Security.Cryptography;
using Domain.Bookings;
using Domain.Qr;
using Infra.Bookings;
using Infra.Qr;
using Infra.Schedules;

namespace App.Qr;

public interface IQrService
{
    Task<string> IssueForBookingAsync(string bookingId, DateTime expUtc);        // Backoffice only
    Task<(string BookingId, bool Valid)> VerifyAsync(string token);               // Operator scan
    Task FinalizeAsync(string bookingId);                                         // Operator finalize
}

public sealed class QrService : IQrService
{
    private readonly IQrRepository _qr;
    private readonly IBookingRepository _bookings;
    private readonly IScheduleRepository _schedules;

    public QrService(IQrRepository qr, IBookingRepository bookings, IScheduleRepository schedules)
    {
        _qr = qr; _bookings = bookings; _schedules = schedules;
    }

    public async Task<string> IssueForBookingAsync(string bookingId, DateTime expUtc)
    {
        //  ensure booking exists and is Approved
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

        // Also confirm booking still active
        var b = await _bookings.GetAsync(t.BookingId);
        if (b is null) return (t.BookingId, false);
        if (b.Status == BookingStatus.Cancelled) return (t.BookingId, false);

        return (t.BookingId, true);
    }

    public async Task FinalizeAsync(string bookingId)
    {
        var b = await _bookings.GetAsync(bookingId) ?? throw new InvalidOperationException("Booking not found.");
        // Set completed + free the slot
        await _bookings.UpdateStatusAsync(bookingId, BookingStatus.Completed);
        await _schedules.SetAvailabilityAsync(b.StationId, b.SlotId, b.StartTimeUtc, true);
    }
}
