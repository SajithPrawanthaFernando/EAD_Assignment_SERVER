// QrToken.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Qr;

public sealed class QrToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("bookingId")]
    public string BookingId { get; set; } = default!;

    [BsonElement("token")]
    public string Token { get; set; } = default!;

    [BsonElement("expUtc")]
    public DateTime ExpUtc { get; set; }
}
