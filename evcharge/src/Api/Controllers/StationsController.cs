using App.Stations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/stations")]
[Authorize]
public class StationsController : ControllerBase
{
    private readonly IStationService _svc;
    public StationsController(IStationService svc) => _svc = svc;

    [HttpPost]
    [Authorize(Roles = "Backoffice")]
    public async Task<ActionResult<string>> Create([FromBody] StationCreateDto dto)
    {
        var id = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    [HttpPut]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Update([FromBody] StationUpdateDto dto)
    {
        await _svc.UpdateAsync(dto);
        return NoContent();
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<StationView>>> GetAll() =>
        Ok(await _svc.GetAllAsync());

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Deactivate(string id)
    {
        try
        {
            await _svc.SetActiveAsync(id, false);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Activate(string id)
    {
        await _svc.SetActiveAsync(id, true);
        return NoContent();
    }
}
