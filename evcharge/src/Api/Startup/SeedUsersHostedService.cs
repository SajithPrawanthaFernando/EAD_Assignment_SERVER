// src/Api/Startup/SeedUsersHostedService.cs
using App.Auth;
using Domain.Users;
using Infra.Users;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Startup;

public sealed class SeedUsersHostedService : IHostedService
{
    private readonly IUserRepository _users;
    private readonly ILogger<SeedUsersHostedService> _logger;

    public SeedUsersHostedService(IUserRepository users, ILogger<SeedUsersHostedService> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            await _users.UpsertSeedUserAsync(
                email: "admin@ev.local",
                passwordHash: PasswordHasher.Hash("Admin#123"),
                roles: new[] { Role.Backoffice });

            await _users.UpsertSeedUserAsync(
                email: "operator@ev.local",
                passwordHash: PasswordHasher.Hash("Operator#123"),
                roles: new[] { Role.StationOperator });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User seed failed");
          
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
