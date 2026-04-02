using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.CreateHabit;

public sealed class CreateHabitCommandHandler(
    IHabitRepository habitRepository,
    IUserRepository userRepository)
    : IRequestHandler<CreateHabitCommand, Result<HabitDto>>
{
    public async Task<Result<HabitDto>> Handle(CreateHabitCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<HabitDto>(Error.NotFound(nameof(User), request.UserId));

        TimeSpan.TryParse(request.TargetTime, out TimeSpan targetTs);

        Habit habit = new()
        {
            UserId        = request.UserId,
            Name          = request.Name.Trim(),
            Description   = request.Description.Trim(),
            TargetTime    = targetTs.ToString(@"hh\:mm\:ss"),
            WindowMinutes = request.WindowMinutes,
            Type          = request.Type,
            Frequency     = new HabitFrequency
            {
                Type           = request.FrequencyType,
                DaysOfWeek     = request.DaysOfWeek,
                TimesPerPeriod = request.TimesPerPeriod,
            },
            CreatedAtUtc  = DateTime.UtcNow,
        };

        Habit created = await habitRepository.CreateAsync(habit, cancellationToken);
        return Result.Success(ToDto(created));
    }

    internal static HabitDto ToDto(Habit h) => new(
        h.Id, h.UserId, h.Name, h.Description, h.TargetTime, h.WindowMinutes, h.Type,
        new HabitFrequencyDto(h.Frequency.Type, h.Frequency.DaysOfWeek, h.Frequency.TimesPerPeriod),
        h.IsActive, h.CreatedAtUtc, "PENDING", null);
}
