using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.DTOs;

public sealed record HabitLogDto(
    string Id,
    string HabitId,
    string UserId,
    DateTime Date,
    HabitLogStatus Status,
    DateTime? RecordedAtUtc,
    string Notes);
