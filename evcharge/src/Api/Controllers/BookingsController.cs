using System.Security.Claims;
using App.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize] // any authenticated user; we check role inside
public class BookingsController : ControllerBase
{
    private readonly IBookingService _svc;
    public BookingsController(IBookingService svc) => _svc = svc;

    private bool IsBackoffice => User.IsInRole("Backoffice");

    // OPTIONAL HOOK: if you later add NIC claim to JWT for EVOwner users,
    // pull it from User.Claims; for now we accept null.
    private string? RequesterNic =>
        User.Claims.FirstOrDefault(c => c.Type == "nic")?.Value;

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
            return Forbid(ex.Message);
        }
    }

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
            return Forbid(ex.Message);
        }
    }

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
            return Forbid(ex.Message);
        }
    }

    [HttpGet("mine/{ownerNic}")]
    public async Task<ActionResult<List<BookingView>>> Mine(string ownerNic)
        => Ok(await _svc.GetMineAsync(ownerNic));

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingView>> GetById(string id)
    {
        var v = await _svc.GetByIdAsync(id);
        return v is null ? NotFound() : Ok(v);
    }
}
