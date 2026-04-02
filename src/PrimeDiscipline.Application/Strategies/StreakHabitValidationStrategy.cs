using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Domain.Entities;

namespace PrimeDiscipline.Application.Strategies;

/// <summary>
/// Streak strategy: the window is narrower (half the configured WindowMinutes)
/// to incentivise more precise timing.
/// </summary>
public sealed class StreakHabitValidationStrategy : IHabitValidationStrategy
{
    public Result<bool> IsVictory(Habit habit, DateTime serverUtcNow, string userTimezone)
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return Result.Failure<bool>(Error.BusinessRule($"Timezone '{userTimezone}' is not recognised."));
        }

        DateTime localNow         = TimeZoneInfo.ConvertTimeFromUtc(serverUtcNow, tz);
        TimeSpan currentTimeOfDay = localNow.TimeOfDay;

        int halfWindow     = Math.Max(1, habit.WindowMinutes / 2);
        TimeSpan rawStart  = habit.TargetTimeOfDay - TimeSpan.FromMinutes(halfWindow);
        TimeSpan rawEnd    = habit.TargetTimeOfDay + TimeSpan.FromMinutes(halfWindow);

        bool isVic = currentTimeOfDay >= rawStart && currentTimeOfDay <= rawEnd;
        return Result.Success(isVic);
    }
}
