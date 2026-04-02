using MongoDB.Driver;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Infrastructure.Persistence.Repositories;

public sealed class HabitRepository(MongoDbContext context) : IHabitRepository
{
    public async Task<Habit?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await context.Habits.Find(h => h.Id == id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Habit>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.Habits.Find(h => h.UserId == userId).ToListAsync(ct);

    public async Task<IReadOnlyList<Habit>> GetAllActiveAsync(CancellationToken ct = default) =>
        await context.Habits.Find(h => h.IsActive).ToListAsync(ct);

    public async Task<Habit> CreateAsync(Habit habit, CancellationToken ct = default)
    {
        await context.Habits.InsertOneAsync(habit, cancellationToken: ct);
        return habit;
    }

    public async Task<bool> UpdateAsync(Habit habit, CancellationToken ct = default)
    {
        UpdateDefinition<Habit> update = Builders<Habit>.Update
            .Set(h => h.Name,          habit.Name)
            .Set(h => h.Description,   habit.Description)
            .Set(h => h.TargetTime,    habit.TargetTime)
            .Set(h => h.WindowMinutes, habit.WindowMinutes)
            .Set(h => h.IsActive,      habit.IsActive)
            .Set(h => h.Frequency,     habit.Frequency);

        UpdateResult result = await context.Habits.UpdateOneAsync(
            h => h.Id == habit.Id, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        DeleteResult result = await context.Habits.DeleteOneAsync(h => h.Id == id, ct);
        return result.DeletedCount > 0;
    }
}
