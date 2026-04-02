using PrimeDiscipline.Domain.Entities;

namespace PrimeDiscipline.Domain.Interfaces;

public interface IHabitRepository
{
    Task<Habit?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Habit>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<Habit>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Habit> CreateAsync(Habit habit, CancellationToken ct = default);
    Task<bool> UpdateAsync(Habit habit, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
