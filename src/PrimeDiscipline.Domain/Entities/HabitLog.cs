using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Domain.Entities;

public sealed class HabitLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("habitId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string HabitId { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>UTC date of the habit day (time component is always 00:00:00).</summary>
    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public HabitLogStatus Status { get; set; }

    /// <summary>UTC timestamp when the action was recorded (null for MISS/auto-DER).</summary>
    [BsonElement("recordedAtUtc")]
    public DateTime? RecordedAtUtc { get; set; }

    [BsonElement("notes")]
    public string Notes { get; set; } = string.Empty;
}
