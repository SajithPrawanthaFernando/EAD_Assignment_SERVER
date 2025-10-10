// BookingController.cs
using System.Security.Claims;
using App.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Qr;

namespace Api.Controllers;

// Booking Route
[ApiController]
[Route("api/bookings")]
[Authorize(Roles = "Backoffice,EVOwner,StationOperator")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _svc;
    private readonly IQrService _qr;
   public BookingsController(IBookingService svc, IQrService qr)
    {
        _svc = svc;
        _qr  = qr; 
    }

    private bool IsBackoffice => User.IsInRole("Backoffice") || User.IsInRole("EVOwner");
    private string? RequesterNic =>
        User.Claims.FirstOrDefault(c => c.Type == "nic")?.Value;

    // Create booking 
    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] BookingCreateDto dto)
    {
        try
        {
            var id = await _svc.CreateAsync(dto, RequesterNic, IsBackoffice);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    // Update booking 
    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] BookingUpdateDto dto)
    {
        try
        {
            await _svc.UpdateAsync(dto, RequesterNic, IsBackoffice);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    // Cancel booking 
    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(string id)
    {
        try
        {
            await _svc.CancelAsync(id, RequesterNic, IsBackoffice);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    // View my own bookings 
    [HttpGet("mine/{ownerNic}")]
    public async Task<ActionResult<List<BookingView>>> Mine(string ownerNic)
    {
        var result = await _svc.GetMineAsync(ownerNic);
        return Ok(result);
    }

    // View booking by id
    [HttpGet("{id}")]
    public async Task<ActionResult<BookingView>> GetById(string id)
    {
        var v = await _svc.GetByIdAsync(id);
        return v is null ? NotFound() : Ok(v);
    }

    // Get all bookings 
    [HttpGet]
    public async Task<ActionResult<List<BookingView>>> GetAll()
    {
        var result = await _svc.GetAllAsync();
        return Ok(result);
    }

    // View my own bookings 
    [HttpGet("mine/{ownerNic}/detail")]
    public async Task<ActionResult<List<BookingWithStationView>>> MineWithStation(string ownerNic)
    {
        var result = await _svc.GetMineWithStationAsync(ownerNic);
        return Ok(result);
    }

    // Approve Bookings
    [HttpPatch("{id}/approve")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Approve(string id)
    {
        try
        {
            await _svc.ApproveAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
    
    // Start Charging Booking
    [HttpPatch("{id}/start-charging")]
    public async Task<IActionResult> StartCharging(string id)
    {
        try
        {
            await _qr.StartChargingAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // Complete Booking
    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(string id)
    {
        try
        {
            await _svc.CompleteAsync(id); 
            return NoContent();
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
