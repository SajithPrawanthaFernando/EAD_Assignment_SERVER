// OwnerDtos.cs
namespace App.EvOwners;

public sealed record EvOwnerUserView(string Email, bool Active, string[] Roles);
public sealed record EvOwnerUpsertDto(
    string Nic,
    string Name,
    string Phone,
    string? Email,
    string? Password
);
public sealed record EvOwnerView(string Nic, string Name, string Phone, string Status, EvOwnerUserView? User);
