using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.DeleteHabit;

public sealed class DeleteHabitCommandHandler(
    IHabitRepository habitRepository,
    IHabitLogRepository habitLogRepository,
    IHabitMonthRepository habitMonthRepository)
    : IRequestHandler<DeleteHabitCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteHabitCommand request, CancellationToken cancellationToken)
    {
        if (!ObjectIdGuard.IsValid(request.HabitId))
            return Result.Failure<bool>(
                Error.Validation(nameof(request.HabitId), "HabitId must be a valid ObjectId."));

        Habit? habit = await habitRepository.GetByIdAsync(request.HabitId, cancellationToken);
        if (habit is null)
            return Result.Failure<bool>(Error.NotFound(nameof(Habit), request.HabitId));

        if (habit.UserId != request.RequestingUserId)
            return Result.Failure<bool>(Error.Forbidden("You do not own this habit."));

        // Delete all associated data in parallel, then the habit itself
        await Task.WhenAll(
            habitLogRepository.DeleteByHabitIdAsync(request.HabitId, cancellationToken),
            habitMonthRepository.DeleteByHabitIdAsync(request.HabitId, cancellationToken));

        await habitRepository.DeleteAsync(request.HabitId, cancellationToken);

        return Result.Success(true);
    }
}
