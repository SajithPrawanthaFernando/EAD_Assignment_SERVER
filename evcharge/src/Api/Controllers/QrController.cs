using App.Qr;
using Infra.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public sealed record IssueQrRequest(string BookingId, DateTime? ExpUtc);
public sealed record IssueQrResponse(string Token);

public sealed record VerifyQrRequest(string Token);
public sealed record VerifyQrResponse(bool Valid, string? BookingId);

[ApiController]
[Route("api/qr")]
public class QrController : ControllerBase
{
    private readonly IQrService _svc;
    private readonly IBookingRepository _bookings;

    public QrController(IQrService svc, IBookingRepository bookings)
    {
        _svc = svc; _bookings = bookings;
    }

    // Backoffice: issue a QR for a booking 
    [Authorize(Roles = "Backoffice,StationOperator")]
    [HttpPost("issue")]
    public async Task<ActionResult<IssueQrResponse>> Issue([FromBody] IssueQrRequest req)
    {
        var b = await _bookings.GetAsync(req.BookingId);
        if (b is null) return NotFound(new { message = "Booking not found" });

        var exp = req.ExpUtc ?? b.StartTimeUtc; // default expiry at start
        var token = await _svc.IssueForBookingAsync(req.BookingId, exp);
        return Ok(new IssueQrResponse(token));
    }

    // Operator: scan token to verify
    [Authorize(Roles = "StationOperator")]
    [HttpPost("verify")]
    public async Task<ActionResult<VerifyQrResponse>> Verify([FromBody] VerifyQrRequest req)
    {
        var (bookingId, valid) = await _svc.VerifyAsync(req.Token);
        return Ok(new VerifyQrResponse(valid, valid ? bookingId : null));
    }

}
