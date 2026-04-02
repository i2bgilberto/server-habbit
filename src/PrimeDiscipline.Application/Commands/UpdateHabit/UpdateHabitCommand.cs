using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.Commands.UpdateHabit;

public sealed record UpdateHabitCommand(
    string HabitId,
    string RequestingUserId,
    string Name,
    string Description,
    string TargetTime,
    int WindowMinutes,
    bool IsActive,
    FrequencyType FrequencyType,
    List<int>? DaysOfWeek,
    int? TimesPerPeriod) : IRequest<Result<HabitDto>>;
