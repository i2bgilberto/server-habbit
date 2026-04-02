using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Domain.Entities;

namespace PrimeDiscipline.Application.Strategies;

/// <summary>
/// Standard strategy: VIC is granted only when the server clock falls inside
/// [TargetTime - WindowMinutes, TargetTime + WindowMinutes].
/// All time comparisons use UTC converted to the user's timezone.
/// </summary>
public sealed class StandardHabitValidationStrategy : IHabitValidationStrategy
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

        DateTime localNow       = TimeZoneInfo.ConvertTimeFromUtc(serverUtcNow, tz);
        TimeSpan currentTimeOfDay = localNow.TimeOfDay;

        TimeSpan rawStart = habit.TargetTimeOfDay - TimeSpan.FromMinutes(habit.WindowMinutes);
        TimeSpan rawEnd   = habit.TargetTimeOfDay + TimeSpan.FromMinutes(habit.WindowMinutes);

        bool isVic = currentTimeOfDay >= rawStart && currentTimeOfDay <= rawEnd;
        return Result.Success(isVic);
    }
}
