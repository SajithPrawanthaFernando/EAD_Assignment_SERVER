namespace App.EvOwners;

public sealed record EvOwnerUpsertDto(string Nic, string Name, string Phone);
public sealed record EvOwnerView(string Nic, string Name, string Phone, string Status);
