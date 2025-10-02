namespace Domain.EvOwners;

public sealed class EvOwner
{
    public string Nic { get; set; } = default!;  
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public EvOwnerStatus Status { get; set; } = EvOwnerStatus.Active;
}
