using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Domain.Interfaces;

public interface IHabitLogRepository
{
    Task<HabitLog?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<HabitLog?> GetByHabitAndDateAsync(string habitId, DateTime date, CancellationToken ct = default);
    Task<IReadOnlyList<HabitLog>> GetByHabitIdAsync(string habitId, CancellationToken ct = default);
    Task<IReadOnlyList<HabitLog>> GetByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>Server-side pagination — avoids loading all logs into memory.</summary>
    Task<(IReadOnlyList<HabitLog> Items, long Total)> GetPagedByHabitIdAsync(
        string habitId, int skip, int take, CancellationToken ct = default);

    /// <summary>Batch lookup: returns existing logs for a set of habitIds on a single date.</summary>
    Task<IReadOnlyList<HabitLog>> GetByHabitIdsAndDateAsync(
        IEnumerable<string> habitIds, DateTime date, CancellationToken ct = default);

    /// <summary>Batch lookup: returns logs for a set of habitIds within a date range (inclusive).</summary>
    Task<IReadOnlyList<HabitLog>> GetByHabitIdsAndDateRangeAsync(
        IEnumerable<string> habitIds, DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>Returns all logs for a given day without a terminal status (used by the background worker).</summary>
    Task<IReadOnlyList<HabitLog>> GetPendingForDateAsync(DateTime date, CancellationToken ct = default);

    Task<HabitLog> CreateAsync(HabitLog log, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(string id, HabitLogStatus status, CancellationToken ct = default);
}
