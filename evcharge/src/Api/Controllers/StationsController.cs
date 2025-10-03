using App.Stations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/stations")]
[Authorize(Roles = "Backoffice")]
public class StationsController : ControllerBase
{
    private readonly IStationService _svc;
    public StationsController(IStationService svc) => _svc = svc;

    [HttpPost]
    public async Task<ActionResult<string>> Create([FromBody] StationCreateDto dto)
    {
        var id = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id }, new { id });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] StationUpdateDto dto)
    {
        await _svc.UpdateAsync(dto);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<StationView>>> GetAll() =>
        Ok(await _svc.GetAllAsync());

    [HttpPatch("{id}/deactivate")]
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
    public async Task<IActionResult> Activate(string id)
    {
        await _svc.SetActiveAsync(id, true);
        return NoContent();
    }
}
