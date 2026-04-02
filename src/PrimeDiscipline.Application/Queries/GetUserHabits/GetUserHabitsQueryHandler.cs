using MediatR;
using PrimeDiscipline.Application.Commands.CreateHabit;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Queries.GetUserHabits;

public sealed class GetUserHabitsQueryHandler(
    IHabitRepository habitRepository,
    IUserRepository userRepository)
    : IRequestHandler<GetUserHabitsQuery, Result<IReadOnlyList<HabitDto>>>
{
    public async Task<Result<IReadOnlyList<HabitDto>>> Handle(
        GetUserHabitsQuery request, CancellationToken cancellationToken)
    {
        if (!ObjectIdGuard.IsValid(request.UserId))
            return Result.Failure<IReadOnlyList<HabitDto>>(
                Error.Validation(nameof(request.UserId), "UserId must be a valid 24-character hex ObjectId."));

        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<IReadOnlyList<HabitDto>>(Error.NotFound(nameof(User), request.UserId));

        IReadOnlyList<Habit> habits = await habitRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        IReadOnlyList<HabitDto> dtos = habits
            .Select(CreateHabitCommandHandler.ToDto)
            .ToList();

        return Result.Success(dtos);
    }
}
