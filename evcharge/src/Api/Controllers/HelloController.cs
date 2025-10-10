// HelloController.cs
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Hello Route
[ApiController]
[Route("api/hello")]
public class HelloController : ControllerBase
{
    // Get Hello
    [HttpGet]
    public IActionResult Get() => Ok(new { message = "EVCharge API up" });
}
