using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PrimeDiscipline.Domain.Entities;

/// <summary>
/// One document per habit per month.
/// Stores progress as bitmasks — zero DB writes for MISS, zero array scans for stats.
/// </summary>
public sealed class HabitMonth
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

    [BsonElement("year")]
    public int Year { get; set; }

    [BsonElement("month")]
    public int Month { get; set; }

    /// <summary>Number of valid bits in this month. Max 31.</summary>
    [BsonElement("bitLength")]
    public int BitLength { get; set; }

    /// <summary>
    /// First day of month this habit counts from.
    /// Bit 0 = startedFromDay, bit 1 = startedFromDay+1, etc.
    /// </summary>
    [BsonElement("startedFromDay")]
    public int StartedFromDay { get; set; }

    /// <summary>Bits = 1 for days that count toward the goal this month.</summary>
    [BsonElement("goalMask")]
    public long GoalMask { get; set; }

    /// <summary>Bits = 1 for days the user completed within the time window.</summary>
    [BsonElement("vicMask")]
    public long VicMask { get; set; }

    /// <summary>Bits = 1 for days the user attempted but outside the window.</summary>
    [BsonElement("derMask")]
    public long DerMask { get; set; }

    /// <summary>
    /// Unix timestamp (seconds) for each bit position.
    /// 0 = no action recorded for that bit.
    /// Length always equals BitLength.
    /// </summary>
    [BsonElement("completionTimes")]
    public long[] CompletionTimes { get; set; } = [];
}
