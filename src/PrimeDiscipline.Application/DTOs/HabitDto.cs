using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.DTOs;

public sealed record HabitFrequencyDto(
    FrequencyType Type,
    List<int>? DaysOfWeek,
    int? TimesPerPeriod);

/// <summary>
/// TodayStatus values:
///   PENDING  — scheduled today, window still open, no log yet
///   VIC      — logged today within the window
///   DER      — logged today outside the window
///   MISS     — scheduled today, window has closed, no log recorded
///   OFF      — not scheduled today (e.g. rest day, wrong weekday)
/// </summary>
public sealed record HabitDto(
    string Id,
    string UserId,
    string Name,
    string Description,
    string TargetTime,
    int WindowMinutes,
    HabitType Type,
    HabitFrequencyDto Frequency,
    bool IsActive,
    DateTime CreatedAtUtc,
    string TodayStatus,
    HabitLogDto? TodayLog);
