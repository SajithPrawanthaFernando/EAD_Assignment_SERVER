// AdminPingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

//Admin Ping
[ApiController]
[Route("api/admin-ping")]
public class AdminPingController : ControllerBase
{
    [Authorize(Roles = "Backoffice")]
    [HttpGet]
    public IActionResult Get() => Ok(new { ok = true, role = "Backoffice" });
}
