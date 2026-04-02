using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Domain.Entities;

public sealed class Habit
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Target time stored as "HH:mm:ss" string for exact, locale-safe serialisation.
    /// Comparisons are always performed server-side against DateTime.UtcNow.
    /// </summary>
    [BsonElement("targetTime")]
    public string TargetTime { get; set; } = "00:00:00";

    /// <summary>Width of the acceptance window in minutes (before and after TargetTime).</summary>
    [BsonElement("windowMinutes")]
    public int WindowMinutes { get; set; } = 30;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public HabitType Type { get; set; } = HabitType.Standard;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("frequency")]
    public HabitFrequency Frequency { get; set; } = new();

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // ── Computed helpers (not persisted) ────────────────────────────────────

    [BsonIgnore]
    public TimeSpan TargetTimeOfDay =>
        TimeSpan.TryParse(TargetTime, out TimeSpan ts) ? ts : TimeSpan.Zero;

    [BsonIgnore]
    public TimeSpan WindowStart => TargetTimeOfDay - TimeSpan.FromMinutes(WindowMinutes);

    [BsonIgnore]
    public TimeSpan WindowEnd => TargetTimeOfDay + TimeSpan.FromMinutes(WindowMinutes);
}
