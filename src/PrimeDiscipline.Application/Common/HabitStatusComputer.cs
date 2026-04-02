using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.Common;

/// <summary>
/// Computes the today-status of a habit without writing anything to the database.
/// Replaces the HabitWindowWorker background cron — pure math, zero DB writes.
/// </summary>
public static class HabitStatusComputer
{
    /// <summary>
    /// Returns the status string for a habit on the current UTC day.
    /// </summary>
    /// <param name="habit">The habit to evaluate.</param>
    /// <param name="todayLog">Actual log for today from the database, or null.</param>
    /// <param name="utcNow">Current UTC time.</param>
    /// <param name="userTimezone">IANA timezone id of the habit owner.</param>
    public static string Compute(Habit habit, HabitLog? todayLog, DateTime utcNow, string userTimezone)
    {
        // If a real log exists just reflect it
        if (todayLog is not null)
            return todayLog.Status.ToString();

        DateTime todayUtc = utcNow.Date;

        // Creation day: tracking starts tomorrow
        if (todayUtc == habit.CreatedAtUtc.Date)
            return "PENDING";

        // Not a scheduled day for this frequency
        if (!IsScheduledForDate(habit, todayUtc))
            return "OFF";

        // Convert current UTC time to the user's local timezone to compare against the window
        TimeSpan localTimeOfDay;
        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            localTimeOfDay = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz).TimeOfDay;
        }
        catch (TimeZoneNotFoundException)
        {
            localTimeOfDay = utcNow.TimeOfDay; // fallback to UTC
        }

        // Window has already closed with no log → MISS
        if (localTimeOfDay > habit.WindowEnd)
            return "MISS";

        // Window is still open (or hasn't started yet)
        return "PENDING";
    }

    /// <summary>
    /// Returns the status string for a specific past date (used for history views).
    /// </summary>
    public static string ComputeForDate(Habit habit, HabitLog? logForDate, DateTime date)
    {
        if (logForDate is not null)
            return logForDate.Status.ToString();

        if (date.Date == habit.CreatedAtUtc.Date)
            return "PENDING";

        if (!IsScheduledForDate(habit, date))
            return "OFF";

        // Past day with no log and was scheduled → MISS
        return "MISS";
    }

    public static bool IsScheduledForDate(Habit habit, DateTime date)
    {
        if (date.Date == habit.CreatedAtUtc.Date)
            return false;

        return habit.Frequency.Type switch
        {
            FrequencyType.Daily         => true,
            FrequencyType.TimesPerWeek  => true,
            FrequencyType.TimesPerMonth => true,
            FrequencyType.Weekdays      => date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday,
            FrequencyType.SixDaysAWeek  => habit.Frequency.DaysOfWeek?.Contains((int)date.DayOfWeek) == true,
            FrequencyType.Custom        => habit.Frequency.DaysOfWeek?.Contains((int)date.DayOfWeek) == true,
            FrequencyType.EveryOtherDay => (int)(date.Date - habit.CreatedAtUtc.Date).TotalDays % 2 == 0,
            _                           => true,
        };
    }
}
