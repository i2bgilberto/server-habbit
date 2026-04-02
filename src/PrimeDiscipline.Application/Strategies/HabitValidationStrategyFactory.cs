using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.Strategies;

/// <summary>
/// Factory that resolves the correct <see cref="IHabitValidationStrategy"/>
/// for a given <see cref="HabitType"/>.
/// </summary>
public sealed class HabitValidationStrategyFactory(
    StandardHabitValidationStrategy standardStrategy,
    StreakHabitValidationStrategy    streakStrategy,
    FlexibleHabitValidationStrategy  flexibleStrategy)
{
    public IHabitValidationStrategy Resolve(HabitType type) => type switch
    {
        HabitType.Standard => standardStrategy,
        HabitType.Streak   => streakStrategy,
        HabitType.Flexible => flexibleStrategy,
        _                  => standardStrategy,
    };
}
