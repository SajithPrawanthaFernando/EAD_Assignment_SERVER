// User.cs
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Users;


public sealed class User
{
    public string Id { get; set; } = default!;

    [BsonElement("email")]
    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;
    public Role[] Roles { get; set; } = Array.Empty<Role>();
    public bool Active { get; set; } = true;

    [BsonElement("ownerNic")]
    public string? OwnerNic { get; set; }
}
