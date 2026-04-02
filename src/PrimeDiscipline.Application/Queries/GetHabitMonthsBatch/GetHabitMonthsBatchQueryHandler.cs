using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Queries.GetHabitMonthsBatch;

public sealed class GetHabitMonthsBatchQueryHandler(
    IHabitRepository habitRepository,
    IHabitMonthRepository habitMonthRepository)
    : IRequestHandler<GetHabitMonthsBatchQuery, Result<IReadOnlyList<HabitMonthDto>>>
{
    public async Task<Result<IReadOnlyList<HabitMonthDto>>> Handle(
        GetHabitMonthsBatchQuery request, CancellationToken cancellationToken)
    {
        // All habits owned by the user
        IReadOnlyList<Habit> habits = await habitRepository.GetByUserIdAsync(
            request.RequestingUserId, cancellationToken);

        if (habits.Count == 0)
            return Result.Success<IReadOnlyList<HabitMonthDto>>([]);

        // All stored month records for this user/period in one DB round-trip
        IReadOnlyList<HabitMonth> records = await habitMonthRepository.GetByUserIdAsync(
            request.RequestingUserId, request.Year, request.Month, cancellationToken);

        Dictionary<string, HabitMonth> byHabitId = records.ToDictionary(r => r.HabitId);

        DateTime utcNow = DateTime.UtcNow;
        List<HabitMonthDto> dtos = new(habits.Count);

        foreach (Habit habit in habits)
        {
            if (!byHabitId.TryGetValue(habit.Id, out HabitMonth? record))
            {
                // No logs this month yet → compute virtual record
                (int startedFromDay, int bitLength) =
                    HabitBitmask.ComputeMonthParams(habit, request.Year, request.Month);

                long goalMask = HabitBitmask.BuildGoalMask(
                    habit, request.Year, request.Month, startedFromDay, bitLength);

                record = new HabitMonth
                {
                    HabitId         = habit.Id,
                    UserId          = habit.UserId,
                    Year            = request.Year,
                    Month           = request.Month,
                    BitLength       = bitLength,
                    StartedFromDay  = startedFromDay,
                    GoalMask        = goalMask,
                    VicMask         = 0,
                    DerMask         = 0,
                    CompletionTimes = new long[bitLength],
                };
            }

            dtos.Add(BuildDto(record, utcNow));
        }

        return Result.Success<IReadOnlyList<HabitMonthDto>>(dtos);
    }

    private static HabitMonthDto BuildDto(HabitMonth m, DateTime utcNow)
    {
        int todayBitIndex = m.Year == utcNow.Year && m.Month == utcNow.Month
            ? HabitBitmask.GetBitIndex(utcNow.Day, m.StartedFromDay)
            : m.BitLength - 1;

        long missMask    = HabitBitmask.ComputeMissMask(m.GoalMask, m.VicMask, m.DerMask, todayBitIndex);
        long pendingMask = HabitBitmask.ComputePendingMask(m.GoalMask, m.VicMask, m.DerMask, todayBitIndex);

        return new HabitMonthDto(
            HabitId:         m.HabitId,
            Year:            m.Year,
            Month:           m.Month,
            BitLength:       m.BitLength,
            StartedFromDay:  m.StartedFromDay,
            GoalMask:        m.GoalMask,
            VicMask:         m.VicMask,
            DerMask:         m.DerMask,
            MissMask:        missMask,
            PendingMask:     pendingMask,
            CompletionTimes: m.CompletionTimes,
            ExpectedCount:   HabitBitmask.GetExpectedCount(m.GoalMask),
            VicCount:        HabitBitmask.GetVicCount(m.VicMask, m.GoalMask),
            DerCount:        HabitBitmask.GetDerCount(m.DerMask, m.GoalMask),
            MissCount:       HabitBitmask.GetMissCount(missMask),
            PendingCount:    HabitBitmask.GetPendingCount(pendingMask),
            VicRate:         HabitBitmask.GetVicRate(m.VicMask, m.GoalMask),
            GoalDays:        HabitBitmask.GetDaysFromMask(m.GoalMask,  m.StartedFromDay, m.BitLength),
            VicDays:         HabitBitmask.GetDaysFromMask(m.VicMask,   m.StartedFromDay, m.BitLength),
            DerDays:         HabitBitmask.GetDaysFromMask(m.DerMask,   m.StartedFromDay, m.BitLength),
            MissDays:        HabitBitmask.GetDaysFromMask(missMask,    m.StartedFromDay, m.BitLength),
            PendingDays:     HabitBitmask.GetDaysFromMask(pendingMask, m.StartedFromDay, m.BitLength)
        );
    }
}
