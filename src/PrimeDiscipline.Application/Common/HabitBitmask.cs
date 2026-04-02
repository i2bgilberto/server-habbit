using System.Numerics;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.Common;

/// <summary>
/// Pure static bitmask math — no DB, no allocations beyond small lists.
/// All masks use long (64-bit signed). Max month = 31 days = 31 bits, well within range.
/// </summary>
public static class HabitBitmask
{
    // ── Mask construction ────────────────────────────────────────────────────

    /// <summary>All valid bits on: 2^bitLength - 1.</summary>
    public static long MaxMask(int bitLength) => (1L << bitLength) - 1;

    /// <summary>
    /// Converts a calendar day-of-month to its zero-based bit index.
    /// Returns -1 if the day is before startedFromDay.
    /// </summary>
    public static int GetBitIndex(int dayOfMonth, int startedFromDay) =>
        dayOfMonth - startedFromDay;

    public static bool IsBitIndexValid(int bitIndex, int bitLength) =>
        bitIndex >= 0 && bitIndex < bitLength;

    // ── Bit operations ───────────────────────────────────────────────────────

    public static long SetBit(long mask, int bitIndex)   => mask |  (1L << bitIndex);
    public static long ClearBit(long mask, int bitIndex) => mask & ~(1L << bitIndex);
    public static bool IsBitSet(long mask, int bitIndex) => (mask & (1L << bitIndex)) != 0;

    /// <summary>
    /// Number of 1-bits in a long.
    /// Uses hardware POPCNT via BitOperations — O(1).
    /// </summary>
    public static int PopCount(long mask) => BitOperations.PopCount((ulong)mask);

    // ── Past/future masks ────────────────────────────────────────────────────

    /// <summary>
    /// Returns a mask with bits 0..todayBitIndex set to 1 (today included).
    /// Used to separate "already passed" from "still pending".
    /// </summary>
    public static long BuildPastDaysMask(int todayBitIndex) =>
        todayBitIndex < 0 ? 0L : (1L << (todayBitIndex + 1)) - 1;

    // ── GoalMask builders ─────────────────────────────────────────────────────

    /// <summary>
    /// Builds the goal_mask for a habit in a given month.
    /// Takes into account frequency, year/month, startedFromDay, and bitLength.
    /// </summary>
    public static long BuildGoalMask(Habit habit, int year, int month, int startedFromDay, int bitLength)
    {
        return habit.Frequency.Type switch
        {
            FrequencyType.Daily         => BuildDailyMask(bitLength),
            FrequencyType.Weekdays      => BuildWeekdaysMask(year, month, startedFromDay, bitLength),
            FrequencyType.SixDaysAWeek  => BuildDaysOfWeekMask(habit.Frequency.DaysOfWeek!, year, month, startedFromDay, bitLength),
            FrequencyType.Custom        => BuildDaysOfWeekMask(habit.Frequency.DaysOfWeek!, year, month, startedFromDay, bitLength),
            FrequencyType.EveryOtherDay => BuildAlternateMask(bitLength),
            FrequencyType.TimesPerWeek  => BuildDailyMask(bitLength),  // every day counts; quota tracked separately
            FrequencyType.TimesPerMonth => BuildDailyMask(bitLength),
            _                           => BuildDailyMask(bitLength),
        };
    }

    /// <summary>All bits on — every day in range is a goal day.</summary>
    private static long BuildDailyMask(int bitLength) => MaxMask(bitLength);

    /// <summary>Only Mon–Fri bits on, based on actual calendar dates.</summary>
    private static long BuildWeekdaysMask(int year, int month, int startedFromDay, int bitLength)
    {
        long mask = 0;
        for (int i = 0; i < bitLength; i++)
        {
            int dayOfMonth = startedFromDay + i;
            DayOfWeek dow  = new DateTime(year, month, dayOfMonth).DayOfWeek;
            if (dow is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                mask = SetBit(mask, i);
        }
        return mask;
    }

    /// <summary>Only the specified DayOfWeek values (0=Sun..6=Sat) bits on.</summary>
    private static long BuildDaysOfWeekMask(
        List<int> daysOfWeek, int year, int month, int startedFromDay, int bitLength)
    {
        long mask = 0;
        for (int i = 0; i < bitLength; i++)
        {
            int dayOfMonth = startedFromDay + i;
            int dow        = (int)new DateTime(year, month, dayOfMonth).DayOfWeek;
            if (daysOfWeek.Contains(dow))
                mask = SetBit(mask, i);
        }
        return mask;
    }

    /// <summary>Alternating: bit 0 = ON, bit 1 = OFF, bit 2 = ON…</summary>
    private static long BuildAlternateMask(int bitLength)
    {
        long mask = 0;
        for (int i = 0; i < bitLength; i += 2)
            mask = SetBit(mask, i);
        return mask;
    }

    // ── Month params helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Computes startedFromDay and bitLength for a habit in a given month.
    /// If the habit was created this month, tracking starts on the creation day.
    /// Otherwise it starts on day 1.
    /// Note: creation-day auto-miss protection lives in HabitStatusComputer,
    /// not here — the bitmask does track the creation day.
    /// </summary>
    public static (int startedFromDay, int bitLength) ComputeMonthParams(
        Habit habit, int year, int month)
    {
        int daysInMonth = DateTime.DaysInMonth(year, month);

        int startedFromDay;

        if (habit.CreatedAtUtc.Year == year && habit.CreatedAtUtc.Month == month)
            startedFromDay = habit.CreatedAtUtc.Day;
        else
            startedFromDay = 1;

        int bitLength = daysInMonth - startedFromDay + 1;
        return (startedFromDay, bitLength);
    }

    // ── Computed stats ───────────────────────────────────────────────────────

    /// <summary>
    /// Days in goal_mask but not yet acted on, up to and including today.
    /// NEVER stored — always computed.
    /// </summary>
    public static long ComputeMissMask(long goalMask, long vicMask, long derMask, int todayBitIndex)
    {
        long pastMask = BuildPastDaysMask(todayBitIndex);
        return goalMask & ~(vicMask | derMask) & pastMask;
    }

    /// <summary>Days that still can be completed (future + today if window still open).</summary>
    public static long ComputePendingMask(long goalMask, long vicMask, long derMask, int todayBitIndex)
    {
        long pastMask = BuildPastDaysMask(todayBitIndex);
        return goalMask & ~(vicMask | derMask) & ~pastMask;
    }

    public static int GetExpectedCount(long goalMask)                  => PopCount(goalMask);
    public static int GetVicCount(long vicMask, long goalMask)         => PopCount(vicMask & goalMask);
    public static int GetDerCount(long derMask, long goalMask)         => PopCount(derMask & goalMask);
    public static int GetMissCount(long missMask)                      => PopCount(missMask);
    public static int GetPendingCount(long pendingMask)                => PopCount(pendingMask);

    public static double GetVicRate(long vicMask, long goalMask)
    {
        int expected = GetExpectedCount(goalMask);
        return expected == 0 ? 0 : (double)GetVicCount(vicMask, goalMask) / expected;
    }

    /// <summary>Returns actual day-of-month numbers for all set bits in a mask.</summary>
    public static List<int> GetDaysFromMask(long mask, int startedFromDay, int bitLength)
    {
        List<int> days = [];
        for (int i = 0; i < bitLength; i++)
            if (IsBitSet(mask, i))
                days.Add(startedFromDay + i);
        return days;
    }

    // ── Validate before writing ───────────────────────────────────────────────

    public static Result<int> ValidateDayWrite(
        int dayOfMonth, int year, int month,
        int startedFromDay, int bitLength, long goalMask)
    {
        int daysInMonth = DateTime.DaysInMonth(year, month);

        if (month < 1 || month > 12)
            return Result.Failure<int>(Error.Validation("month", "Month must be between 1 and 12."));

        if (year < 2000 || year > 2100)
            return Result.Failure<int>(Error.Validation("year", "Year out of valid range."));

        if (dayOfMonth < 1 || dayOfMonth > daysInMonth)
            return Result.Failure<int>(Error.Validation("day", $"Day {dayOfMonth} is not valid for {year}-{month:D2}."));

        int bitIndex = GetBitIndex(dayOfMonth, startedFromDay);

        if (!IsBitIndexValid(bitIndex, bitLength))
            return Result.Failure<int>(Error.Validation("day", $"Day {dayOfMonth} is outside this habit's tracking range (starts day {startedFromDay})."));

        if (!IsBitSet(goalMask, bitIndex))
            return Result.Failure<int>(Error.BusinessRule($"Day {dayOfMonth} is not a goal day for this habit's schedule."));

        return Result.Success(bitIndex);
    }
}
