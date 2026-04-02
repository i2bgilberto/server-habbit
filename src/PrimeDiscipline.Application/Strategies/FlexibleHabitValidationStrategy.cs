using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Domain.Entities;

namespace PrimeDiscipline.Application.Strategies;

/// <summary>
/// Flexible strategy: any attempt during the calendar day counts as VIC.
/// </summary>
public sealed class FlexibleHabitValidationStrategy : IHabitValidationStrategy
{
    public Result<bool> IsVictory(Habit habit, DateTime serverUtcNow, string userTimezone)
    {
        // Any in-day attempt is always a victory for Flexible habits.
        return Result.Success(true);
    }
}
