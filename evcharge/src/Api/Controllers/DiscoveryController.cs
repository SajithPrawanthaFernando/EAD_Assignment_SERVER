using App.Stations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/stations")]
public class DiscoveryController : ControllerBase
{
    private readonly INearbyStations _nearby;
    public DiscoveryController(INearbyStations nearby) => _nearby = nearby;

  
    [HttpGet("nearby")]
    public async Task<ActionResult<List<StationView>>> Nearby([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double rKm = 10)
        => Ok(await _nearby.FindAsync(lat, lng, rKm));
}
