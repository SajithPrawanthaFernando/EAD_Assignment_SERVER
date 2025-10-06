using App.Auth;                 
using Domain.EvOwners;
using Domain.Users;             
using Infra.EvOwners;
using Infra.Users;              
using MongoDB.Driver;           

namespace App.EvOwners;

public interface IEvOwnerService
{
    Task UpsertAsync(EvOwnerUpsertDto dto);
    Task DeactivateAsync(string nic);
    Task ReactivateAsync(string nic);
    Task<EvOwnerView?> GetAsync(string nic);
}

public sealed class EvOwnerService : IEvOwnerService
{
    private readonly IEvOwnerRepository _repo;
    private readonly IUserRepository _users;

    public EvOwnerService(IEvOwnerRepository repo, IUserRepository users)
    {
        _repo = repo;
        _users = users;
    }

    public async Task UpsertAsync(EvOwnerUpsertDto dto)
    {
        // Upsert owner
        var o = await _repo.GetAsync(dto.Nic) ?? new EvOwner { Nic = dto.Nic };
        o.Name = dto.Name;
        o.Phone = dto.Phone;
        await _repo.UpsertAsync(o);

       
        var normalizedEmail = dto.Email?.Trim().ToLowerInvariant();

        
        User? existingUser = null;
        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            existingUser = await _users.FindByEmailAsync(normalizedEmail);
        }

        if (existingUser is null)
        {
           
            if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidOperationException("Email and password are required to create a new owner user.");

          var newUser = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = normalizedEmail!,
            PasswordHash = PasswordHasher.Hash(dto.Password!),
            Roles = new[] { Role.EVOwner },
            Active = true,
            OwnerNic = dto.Nic        
        };

            await _users.InsertAsync(newUser);
        }
        
    }
   public async Task DeactivateAsync(string nic)
    {
        // Deactivate the owner
        await _repo.SetStatusAsync(nic, EvOwnerStatus.Deactivated);

        // Find and deactivate the linked user
        var user = await _users.Collection.Find(x => x.OwnerNic == nic).FirstOrDefaultAsync();
        if (user is not null && user.Active)
        {
            user.Active = false;
            await _users.Collection.ReplaceOneAsync(x => x.Id == user.Id, user);
        }
    }

    public async Task ReactivateAsync(string nic)
    {
        // Reactivate the owner
        await _repo.SetStatusAsync(nic, EvOwnerStatus.Active);

        // Find and reactivate the linked user
        var user = await _users.Collection.Find(x => x.OwnerNic == nic).FirstOrDefaultAsync();
        if (user is not null && !user.Active)
        {
            user.Active = true;
            await _users.Collection.ReplaceOneAsync(x => x.Id == user.Id, user);
        }
    }


   public async Task<EvOwnerView?> GetAsync(string nic)
{
    var owner = await _repo.GetAsync(nic);
    if (owner is null)
        return null;

    var user = await _users.Collection.Find(x => x.OwnerNic == nic).FirstOrDefaultAsync();

    EvOwnerUserView? userView = null;
    if (user is not null)
    {
        userView = new EvOwnerUserView(
            Email: user.Email,
            Active: user.Active,
            Roles: user.Roles.Select(r => r.ToString()).ToArray()
        );
    }

    return new EvOwnerView(
        Nic: owner.Nic,
        Name: owner.Name,
        Phone: owner.Phone,
        Status: owner.Status.ToString(),
        User: userView
    );
}

}
