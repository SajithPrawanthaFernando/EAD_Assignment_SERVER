//EvOwnersController.cs
using App.EvOwners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

//Ev Owners Route
[ApiController]
[Route("api/ev-owners")]
public class EvOwnersController : ControllerBase
{
    private readonly IEvOwnerService _svc;
    public EvOwnersController(IEvOwnerService svc) => _svc = svc;

    // Update
    [HttpPut] 
    [Authorize]
    public async Task<IActionResult> Upsert([FromBody] EvOwnerUpsertDto dto)
    {
        await _svc.UpsertAsync(dto);
        return NoContent();
    }

    // Deactivate
    [HttpPatch("{nic}/deactivate")]
    [Authorize]
    public async Task<IActionResult> Deactivate(string nic)
    {
        await _svc.DeactivateAsync(nic);
        return NoContent();
    }

    // Reactivate
    [HttpPatch("{nic}/reactivate")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Reactivate(string nic)
    {
        await _svc.ReactivateAsync(nic);
        return NoContent();
    }
    
    // Get By NIC
    [HttpGet("{nic}")]
    [Authorize]
    public async Task<ActionResult<EvOwnerView>> Get(string nic)
    {
        var v = await _svc.GetAsync(nic);
        return v is null ? NotFound() : Ok(v);
    }

    // Get All
    [HttpGet]
    [Authorize(Roles = "Backoffice")]
    public async Task<ActionResult<List<EvOwnerView>>> GetAll()
           => Ok(await _svc.GetAllAsync());

    // Delete By NIC        
    [HttpDelete("{nic}")]
    [Authorize(Roles = "Backoffice")]
    public async Task<IActionResult> Delete(string nic)
    {
        await _svc.DeleteAsync(nic);
        return NoContent(); 
    }        
}
