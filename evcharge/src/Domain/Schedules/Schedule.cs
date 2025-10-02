// src/Domain/Schedules/Schedule.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Schedules;


public sealed class Schedule
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]  
    public string Id { get; set; } = default!;

    public string StationId { get; set; } = default!;
    public string SlotId { get; set; } = default!;
    public DateTime StartTimeUtc { get; set; }
    public bool IsAvailable { get; set; } = true;
}
