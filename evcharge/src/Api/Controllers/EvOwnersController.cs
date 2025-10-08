using App.EvOwners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/ev-owners")]
public class EvOwnersController : ControllerBase
{
    private readonly IEvOwnerService _svc;
    public EvOwnersController(IEvOwnerService svc) => _svc = svc;

    [HttpPut] // upsert
    [Authorize]
    public async Task<IActionResult> Upsert([FromBody] EvOwnerUpsertDto dto)
    {
        await _svc.UpsertAsync(dto);
        return NoContent();
    }

    [HttpPatch("{nic}/deactivate")]
    [Authorize]
    public async Task<IActionResult> Deactivate(string nic)
    {
        await _svc.DeactivateAsync(nic);
        return NoContent();
    }

    [HttpPatch("{nic}/reactivate")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Reactivate(string nic)
    {
        await _svc.ReactivateAsync(nic);
        return NoContent();
    }

    [HttpGet("{nic}")]
    [Authorize]
    public async Task<ActionResult<EvOwnerView>> Get(string nic)
    {
        var v = await _svc.GetAsync(nic);
        return v is null ? NotFound() : Ok(v);
    }

    [HttpGet]
    [Authorize(Roles = "Backoffice")]
    public async Task<ActionResult<List<EvOwnerView>>> GetAll()
           => Ok(await _svc.GetAllAsync());
            
    [HttpDelete("{nic}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Delete(string nic)
    {
        await _svc.DeleteAsync(nic);
        return NoContent(); 
    }        
}
