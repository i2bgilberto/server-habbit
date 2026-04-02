using MediatR;
using PrimeDiscipline.Application.Commands.CreateHabit;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.UpdateHabit;

public sealed class UpdateHabitCommandHandler(IHabitRepository habitRepository)
    : IRequestHandler<UpdateHabitCommand, Result<HabitDto>>
{
    public async Task<Result<HabitDto>> Handle(UpdateHabitCommand request, CancellationToken cancellationToken)
    {
        Habit? habit = await habitRepository.GetByIdAsync(request.HabitId, cancellationToken);
        if (habit is null)
            return Result.Failure<HabitDto>(Error.NotFound(nameof(Habit), request.HabitId));

        if (habit.UserId != request.RequestingUserId)
            return Result.Failure<HabitDto>(Error.Forbidden("You do not own this habit."));

        TimeSpan.TryParse(request.TargetTime, out TimeSpan ts);

        habit.Name          = request.Name.Trim();
        habit.Description   = request.Description.Trim();
        habit.TargetTime    = ts.ToString(@"hh\:mm\:ss");
        habit.WindowMinutes = request.WindowMinutes;
        habit.IsActive      = request.IsActive;
        habit.Frequency     = new HabitFrequency
        {
            Type           = request.FrequencyType,
            DaysOfWeek     = request.DaysOfWeek,
            TimesPerPeriod = request.TimesPerPeriod,
        };

        await habitRepository.UpdateAsync(habit, cancellationToken);
        return Result.Success(CreateHabitCommandHandler.ToDto(habit));
    }
}
