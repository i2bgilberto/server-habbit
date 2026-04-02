using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Queries.GetHabitMonth;

public sealed class GetHabitMonthQueryHandler(
    IHabitRepository habitRepository,
    IHabitMonthRepository habitMonthRepository)
    : IRequestHandler<GetHabitMonthQuery, Result<HabitMonthDto>>
{
    public async Task<Result<HabitMonthDto>> Handle(
        GetHabitMonthQuery request, CancellationToken cancellationToken)
    {
        if (!ObjectIdGuard.IsValid(request.HabitId))
            return Result.Failure<HabitMonthDto>(
                Error.Validation(nameof(request.HabitId), "HabitId must be a valid ObjectId."));

        Habit? habit = await habitRepository.GetByIdAsync(request.HabitId, cancellationToken);
        if (habit is null)
            return Result.Failure<HabitMonthDto>(Error.NotFound(nameof(Habit), request.HabitId));

        if (habit.UserId != request.RequestingUserId)
            return Result.Failure<HabitMonthDto>(Error.Forbidden("You do not own this habit."));

        HabitMonth? record = await habitMonthRepository.GetAsync(
            request.HabitId, request.Year, request.Month, cancellationToken);

        // If no record yet, compute a virtual one from the habit definition
        if (record is null)
        {
            (int startedFromDay, int bitLength) =
                HabitBitmask.ComputeMonthParams(habit, request.Year, request.Month);

            long goalMask = HabitBitmask.BuildGoalMask(
                habit, request.Year, request.Month, startedFromDay, bitLength);

            record = new HabitMonth
            {
                HabitId        = habit.Id,
                UserId         = habit.UserId,
                Year           = request.Year,
                Month          = request.Month,
                BitLength      = bitLength,
                StartedFromDay = startedFromDay,
                GoalMask       = goalMask,
                VicMask        = 0,
                DerMask        = 0,
                CompletionTimes = new long[bitLength],
            };
        }

        return Result.Success(BuildDto(record));
    }

    private static HabitMonthDto BuildDto(HabitMonth m)
    {
        DateTime utcNow      = DateTime.UtcNow;
        int todayBitIndex    = m.Year == utcNow.Year && m.Month == utcNow.Month
            ? HabitBitmask.GetBitIndex(utcNow.Day, m.StartedFromDay)
            : m.BitLength - 1; // past month → all days are "past"

        long missMask    = HabitBitmask.ComputeMissMask(m.GoalMask, m.VicMask, m.DerMask, todayBitIndex);
        long pendingMask = HabitBitmask.ComputePendingMask(m.GoalMask, m.VicMask, m.DerMask, todayBitIndex);

        return new HabitMonthDto(
            HabitId:        m.HabitId,
            Year:           m.Year,
            Month:          m.Month,
            BitLength:      m.BitLength,
            StartedFromDay: m.StartedFromDay,
            GoalMask:       m.GoalMask,
            VicMask:        m.VicMask,
            DerMask:        m.DerMask,
            MissMask:       missMask,
            PendingMask:    pendingMask,
            CompletionTimes: m.CompletionTimes,
            ExpectedCount:  HabitBitmask.GetExpectedCount(m.GoalMask),
            VicCount:       HabitBitmask.GetVicCount(m.VicMask, m.GoalMask),
            DerCount:       HabitBitmask.GetDerCount(m.DerMask, m.GoalMask),
            MissCount:      HabitBitmask.GetMissCount(missMask),
            PendingCount:   HabitBitmask.GetPendingCount(pendingMask),
            VicRate:        HabitBitmask.GetVicRate(m.VicMask, m.GoalMask),
            GoalDays:       HabitBitmask.GetDaysFromMask(m.GoalMask,  m.StartedFromDay, m.BitLength),
            VicDays:        HabitBitmask.GetDaysFromMask(m.VicMask,   m.StartedFromDay, m.BitLength),
            DerDays:        HabitBitmask.GetDaysFromMask(m.DerMask,   m.StartedFromDay, m.BitLength),
            MissDays:       HabitBitmask.GetDaysFromMask(missMask,    m.StartedFromDay, m.BitLength),
            PendingDays:    HabitBitmask.GetDaysFromMask(pendingMask, m.StartedFromDay, m.BitLength)
        );
    }
}
