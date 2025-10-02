using Domain.EvOwners;
using Infra.EvOwners;

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
    public EvOwnerService(IEvOwnerRepository repo) => _repo = repo;

    public async Task UpsertAsync(EvOwnerUpsertDto dto)
    {
        var o = await _repo.GetAsync(dto.Nic) ?? new EvOwner { Nic = dto.Nic };
        o.Name = dto.Name;
        o.Phone = dto.Phone;
        // retain existing status or default Active
        await _repo.UpsertAsync(o);
    }

    public Task DeactivateAsync(string nic) => _repo.SetStatusAsync(nic, EvOwnerStatus.Deactivated);
    public Task ReactivateAsync(string nic) => _repo.SetStatusAsync(nic, EvOwnerStatus.Active);

    public async Task<EvOwnerView?> GetAsync(string nic)
    {
        var o = await _repo.GetAsync(nic);
        return o is null ? null : new EvOwnerView(o.Nic, o.Name, o.Phone, o.Status.ToString());
    }
}
