using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.Commands.CreateHabit;

public sealed record CreateHabitCommand(
    string UserId,
    string Name,
    string Description,
    string TargetTime,
    int WindowMinutes,
    HabitType Type,
    FrequencyType FrequencyType,
    List<int>? DaysOfWeek,
    int? TimesPerPeriod) : IRequest<Result<HabitDto>>;
