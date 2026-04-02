using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Domain.Entities;

namespace PrimeDiscipline.Application.Strategies;

/// <summary>
/// Determines whether a completion attempt at <paramref name="serverUtcNow"/> is
/// a Victory (VIC) or a Defeat (DER) for the given <paramref name="habit"/>.
/// </summary>
public interface IHabitValidationStrategy
{
    Result<bool> IsVictory(Habit habit, DateTime serverUtcNow, string userTimezone);
}
