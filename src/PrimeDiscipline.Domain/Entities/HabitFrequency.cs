using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Domain.Entities;

/// <summary>
/// Embedded document that defines when a habit must be performed.
/// Stored inside the Habit document — no separate collection.
/// </summary>
public sealed class HabitFrequency
{
    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public FrequencyType Type { get; set; } = FrequencyType.Daily;

    /// <summary>
    /// Used by SixDaysAWeek and Custom.
    /// Values are DayOfWeek integers: 0=Sunday … 6=Saturday.
    /// </summary>
    [BsonElement("daysOfWeek")]
    public List<int>? DaysOfWeek { get; set; }

    /// <summary>
    /// Used by TimesPerWeek (1–7) and TimesPerMonth (1–31).
    /// Defines the VIC quota for the period; every calendar day without a log is still a MISS.
    /// </summary>
    [BsonElement("timesPerPeriod")]
    public int? TimesPerPeriod { get; set; }
}
