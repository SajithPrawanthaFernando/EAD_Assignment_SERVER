using App.Auth;
using Domain.Users;
using Infra.Users;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Api.Startup;

public sealed class SeedUsersHostedService : IHostedService
{
    private readonly IUserRepository _users;
    public SeedUsersHostedService(IUserRepository users) => _users = users;

    public async Task StartAsync(CancellationToken ct)
    {
        // Backoffice
        var admin = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = "admin@ev.local",
            PasswordHash = PasswordHasher.Hash("Admin#123"),
            Roles = new[] { Role.Backoffice },
            Active = true
        };

        await _users.Collection.ReplaceOneAsync(
            Builders<User>.Filter.Eq(u => u.Email, admin.Email),
            admin,
            new ReplaceOptions { IsUpsert = true },
            ct);

        // Station Operator
        var op = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = "operator@ev.local",
            PasswordHash = PasswordHasher.Hash("Operator#123"),
            Roles = new[] { Role.StationOperator },
            Active = true
        };

        await _users.Collection.ReplaceOneAsync(
            Builders<User>.Filter.Eq(u => u.Email, op.Email),
            op,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
