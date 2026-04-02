using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.Commands.RecordActivity;

/// <summary>
/// Factory responsible for instantiating <see cref="HabitLog"/> records.
/// Centralises creation logic for VIC, DER and MISS statuses.
/// </summary>
public static class HabitLogFactory
{
    public static HabitLog Create(
        string habitId,
        string userId,
        DateTime date,
        HabitLogStatus status,
        DateTime? recordedAtUtc,
        string notes = "") => new()
    {
        HabitId       = habitId,
        UserId        = userId,
        Date          = date.Date,
        Status        = status,
        RecordedAtUtc = recordedAtUtc,
        Notes         = notes,
    };

    public static HabitLog CreateMiss(string habitId, string userId, DateTime date) =>
        Create(habitId, userId, date, HabitLogStatus.MISS, recordedAtUtc: null);

    public static HabitLog CreateAutoDefeat(string habitId, string userId, DateTime date) =>
        Create(habitId, userId, date, HabitLogStatus.DER, recordedAtUtc: null, "Auto-closed by background worker.");
}
