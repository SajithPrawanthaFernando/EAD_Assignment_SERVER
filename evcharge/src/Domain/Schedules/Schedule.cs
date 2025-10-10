// Schedule.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Schedules;

public sealed class Schedule
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("stationId")]
    public string StationId { get; set; } = default!;

    [BsonElement("slotId")]
    public string SlotId { get; set; } = default!;

    [BsonElement("startTimeUtc")]
    public DateTime StartTimeUtc { get; set; }

    [BsonElement("isAvailable")]
    public bool IsAvailable { get; set; } = true;
}
