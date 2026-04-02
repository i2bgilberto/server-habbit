using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.DTOs;

public sealed record HabitFrequencyDto(
    FrequencyType Type,
    List<int>? DaysOfWeek,
    int? TimesPerPeriod);

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
    DateTime CreatedAtUtc);
