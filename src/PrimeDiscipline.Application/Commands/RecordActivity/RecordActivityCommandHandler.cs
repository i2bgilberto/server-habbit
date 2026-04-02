using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Application.Strategies;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Enums;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Commands.RecordActivity;

public sealed class RecordActivityCommandHandler(
    IHabitRepository habitRepository,
    IHabitLogRepository habitLogRepository,
    IHabitMonthRepository habitMonthRepository,
    IUserRepository userRepository,
    HabitValidationStrategyFactory strategyFactory)
    : IRequestHandler<RecordActivityCommand, Result<HabitLogDto>>
{
    public async Task<Result<HabitLogDto>> Handle(
        RecordActivityCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Load and validate ─────────────────────────────────────────────
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

        // ── 2. Prevent duplicate logs for the same calendar day ──────────────
        DateTime serverUtcNow = DateTime.UtcNow;
        DateTime todayUtc     = serverUtcNow.Date;

        HabitLog? existing = await habitLogRepository.GetByHabitAndDateAsync(
            request.HabitId, todayUtc, cancellationToken);

        if (existing is not null && existing.Status == HabitLogStatus.VIC)
            return Result.Failure<HabitLogDto>(
                Error.Conflict("Activity has already been recorded as VIC for today."));

        // ── 3. Resolve strategy & determine VIC/DER ──────────────────────────
        IHabitValidationStrategy strategy = strategyFactory.Resolve(habit.Type);
        Result<bool> validationResult     = strategy.IsVictory(habit, serverUtcNow, user.Timezone);

        if (validationResult.IsFailure)
            return Result.Failure<HabitLogDto>(validationResult.Errors);

        HabitLogStatus status = validationResult.Value! ? HabitLogStatus.VIC : HabitLogStatus.DER;

        // ── 4. Persist the log ───────────────────────────────────────────────
        HabitLog log = HabitLogFactory.Create(
            request.HabitId, request.UserId, todayUtc,
            status, serverUtcNow, request.Notes ?? string.Empty);

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

        // ── 5. Update the bitmask monthly record (fire-and-update) ───────────
        await UpdateHabitMonthAsync(habit, user, todayUtc, serverUtcNow, status, cancellationToken);

        return Result.Success(new HabitLogDto(
            created.Id, created.HabitId, created.UserId,
            created.Date, created.Status, created.RecordedAtUtc, created.Notes));
    }

    private async Task UpdateHabitMonthAsync(
        Habit habit, User user,
        DateTime todayUtc, DateTime recordedAt,
        HabitLogStatus status,
        CancellationToken ct)
    {
        int year  = todayUtc.Year;
        int month = todayUtc.Month;

        (int startedFromDay, int bitLength) = HabitBitmask.ComputeMonthParams(habit, year, month);

        // Day is before the tracking range (creation day) — skip
        int bitIndex = HabitBitmask.GetBitIndex(todayUtc.Day, startedFromDay);
        if (!HabitBitmask.IsBitIndexValid(bitIndex, bitLength))
            return;

        long goalMask      = HabitBitmask.BuildGoalMask(habit, year, month, startedFromDay, bitLength);
        long unixTimestamp = new DateTimeOffset(recordedAt).ToUnixTimeSeconds();

        await habitMonthRepository.UpsertDayAsync(
            habit.Id, habit.UserId,
            year, month, bitLength, startedFromDay, goalMask,
            bitIndex,
            isVic: status == HabitLogStatus.VIC,
            unixTimestamp,
            ct);
    }
}
