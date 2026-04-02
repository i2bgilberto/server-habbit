using PrimeDiscipline.Domain.Entities;

namespace PrimeDiscipline.Domain.Interfaces;

public interface IHabitMonthRepository
{
    Task<HabitMonth?> GetAsync(string habitId, int year, int month, CancellationToken ct = default);
    Task<IReadOnlyList<HabitMonth>> GetByHabitIdAsync(string habitId, CancellationToken ct = default);
    Task<IReadOnlyList<HabitMonth>> GetByUserIdAsync(string userId, int year, int month, CancellationToken ct = default);

    /// <summary>
    /// Atomically sets vicMask or derMask bit and records the completion timestamp.
    /// Creates the document on first write (upsert).
    /// </summary>
    Task DeleteByHabitIdAsync(string habitId, CancellationToken ct = default);

    Task UpsertDayAsync(
        string habitId,
        string userId,
        int year,
        int month,
        int bitLength,
        int startedFromDay,
        long goalMask,
        int bitIndex,
        bool isVic,
        long unixTimestamp,
        CancellationToken ct = default);
}
