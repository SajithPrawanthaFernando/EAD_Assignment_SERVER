// UsersController.cs
using App.Users;
using Infra.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Users Route
[ApiController]
[Route("api/users")]
[Authorize]                
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _users;
    public UsersController(IUserRepository users) => _users = users;

    // Get all users
    [HttpGet]
    public async Task<ActionResult<List<UserView>>> GetAll()
    {
        var all = await _users.GetAllAsync();
        return Ok(all.Select(UserMap.ToView).ToList());
    }

    // get by id
    [HttpGet("{id}")]
    public async Task<ActionResult<UserView>> GetById(string id)
    {
        var u = await _users.GetByIdAsync(id);
        return u is null ? NotFound() : Ok(UserMap.ToView(u));
    }

    // get by nic
    [HttpGet("by-nic/{nic}")]
    public async Task<ActionResult<UserView>> GetByNic(string nic)
    {
        var u = await _users.FindByOwnerNicAsync(nic);
        return u is null ? NotFound() : Ok(UserMap.ToView(u));
    }
}