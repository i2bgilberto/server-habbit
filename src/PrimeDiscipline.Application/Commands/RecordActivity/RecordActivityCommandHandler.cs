using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Application.Strategies;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Enums;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.RecordActivity;

/// <summary>
/// Core business-logic handler.
/// Validates the time window server-side using DateTime.UtcNow exclusively.
/// </summary>
public sealed class RecordActivityCommandHandler(
    IHabitRepository habitRepository,
    IHabitLogRepository habitLogRepository,
    IUserRepository userRepository,
    HabitValidationStrategyFactory strategyFactory)
    : IRequestHandler<RecordActivityCommand, Result<HabitLogDto>>
{
    public async Task<Result<HabitLogDto>> Handle(
        RecordActivityCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Load and validate the entities ───────────────────────────────
        Habit? habit = await habitRepository.GetByIdAsync(request.HabitId, cancellationToken);
        if (habit is null)
            return Result.Failure<HabitLogDto>(Error.NotFound(nameof(Habit), request.HabitId));

        if (!habit.IsActive)
            return Result.Failure<HabitLogDto>(Error.BusinessRule("This habit is inactive."));

        if (habit.UserId != request.UserId)
            return Result.Failure<HabitLogDto>(Error.Forbidden("You do not own this habit."));

        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<HabitLogDto>(Error.NotFound(nameof(User), request.UserId));

        // ── 2. Prevent duplicate logs for the same calendar day ─────────────
        DateTime serverUtcNow = DateTime.UtcNow;
        DateTime todayUtc     = serverUtcNow.Date;

        HabitLog? existing = await habitLogRepository.GetByHabitAndDateAsync(
            request.HabitId, todayUtc, cancellationToken);

        if (existing is not null && existing.Status == HabitLogStatus.VIC)
            return Result.Failure<HabitLogDto>(
                Error.Conflict("Activity has already been recorded as VIC for today."));

        // ── 3. Resolve strategy & determine outcome ─────────────────────────
        IHabitValidationStrategy strategy = strategyFactory.Resolve(habit.Type);
        Result<bool> validationResult     = strategy.IsVictory(habit, serverUtcNow, user.Timezone);

        if (validationResult.IsFailure)
            return Result.Failure<HabitLogDto>(validationResult.Errors);

        HabitLogStatus status = validationResult.Value! ? HabitLogStatus.VIC : HabitLogStatus.DER;

        // ── 4. Factory: build log entity ─────────────────────────────────────
        HabitLog log = HabitLogFactory.Create(
            request.HabitId,
            request.UserId,
            todayUtc,
            status,
            serverUtcNow,
            request.Notes);

        HabitLog created;
        try
        {
            created = await habitLogRepository.CreateAsync(log, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<HabitLogDto>(
                Error.Conflict("A log already exists for this habit today."));
        }

        HabitLogDto dto = new(
            created.Id,
            created.HabitId,
            created.UserId,
            created.Date,
            created.Status,
            created.RecordedAtUtc,
            created.Notes);

        return Result.Success(dto);
    }
}
