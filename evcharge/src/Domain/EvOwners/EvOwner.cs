// EvOwner.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.EvOwners;

[BsonIgnoreExtraElements]
public sealed class EvOwner
{
    
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("nic")]
    public string Nic { get; set; } = default!; 

    [BsonElement("name")]
    public string Name { get; set; } = default!;

    [BsonElement("phone")]
    public string Phone { get; set; } = default!;

    [BsonElement("status")]
    public EvOwnerStatus Status { get; set; } = EvOwnerStatus.Active;
}
