namespace PrimeDiscipline.Domain.Enums;

/// <summary>
/// VIC = Victoria (completed within the time window)
/// DER = Derrota (window expired without a completion attempt)
/// MISS = Missed (habit not attempted at all during the day)
/// </summary>
public enum HabitLogStatus
{
    VIC  = 0,
    DER  = 1,
    MISS = 2,
}
