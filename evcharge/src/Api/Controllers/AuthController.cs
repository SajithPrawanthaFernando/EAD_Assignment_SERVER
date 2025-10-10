// AuthController.cs
using App.Auth;
using Infra.Users;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token);

// Auth Route
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IJwtIssuer _jwt;

    public AuthController(IUserRepository users, IJwtIssuer jwt)
    {
        _users = users; _jwt = jwt;
    }

    // Login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var u = await _users.FindByEmailAsync(req.Email.Trim().ToLowerInvariant());
        if (u is null || !u.Active) return Unauthorized(new { message = "Invalid credentials." });
        if (!PasswordHasher.Verify(req.Password, u.PasswordHash)) return Unauthorized(new { message = "Invalid credentials." });

        var token = _jwt.Issue(u.Id, u.Email, u.Roles);
        return Ok(new LoginResponse(token));
    }
}
