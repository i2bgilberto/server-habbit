using MongoDB.Driver;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Infrastructure.Persistence.Repositories;

public sealed class HabitMonthRepository(MongoDbContext context) : IHabitMonthRepository
{
    public async Task<HabitMonth?> GetAsync(
        string habitId, int year, int month, CancellationToken ct = default) =>
        await context.HabitMonths
            .Find(m => m.HabitId == habitId && m.Year == year && m.Month == month)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<HabitMonth>> GetByHabitIdAsync(
        string habitId, CancellationToken ct = default) =>
        await context.HabitMonths
            .Find(m => m.HabitId == habitId)
            .SortByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HabitMonth>> GetByUserIdAsync(
        string userId, int year, int month, CancellationToken ct = default) =>
        await context.HabitMonths
            .Find(m => m.UserId == userId && m.Year == year && m.Month == month)
            .ToListAsync(ct);

    public async Task DeleteByHabitIdAsync(string habitId, CancellationToken ct = default) =>
        await context.HabitMonths.DeleteManyAsync(m => m.HabitId == habitId, ct);

    public async Task UpsertDayAsync(
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
        CancellationToken ct = default)
    {
        FilterDefinition<HabitMonth> filter = Builders<HabitMonth>.Filter.And(
            Builders<HabitMonth>.Filter.Eq(m => m.HabitId, habitId),
            Builders<HabitMonth>.Filter.Eq(m => m.Year, year),
            Builders<HabitMonth>.Filter.Eq(m => m.Month, month));

        bool docExists = await context.HabitMonths.Find(filter).AnyAsync(ct);

        if (!docExists)
        {
            // First log this month: create a fully-initialized document.
            // We do not use $setOnInsert + $bit in the same update because
            // targeting the same field path with two operators is a conflict.
            long[] times = new long[bitLength];
            times[bitIndex] = unixTimestamp;

            HabitMonth newDoc = new()
            {
                HabitId        = habitId,
                UserId         = userId,
                Year           = year,
                Month          = month,
                BitLength      = bitLength,
                StartedFromDay = startedFromDay,
                GoalMask       = goalMask,
                VicMask        = isVic ? (1L << bitIndex) : 0L,
                DerMask        = isVic ? 0L : (1L << bitIndex),
                CompletionTimes = times,
            };

            try
            {
                await context.HabitMonths.InsertOneAsync(newDoc, null, ct);
                return;
            }
            catch (MongoDB.Driver.MongoWriteException ex)
                when (ex.WriteError?.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey)
            {
                // Race condition: another request inserted the document first.
                // Fall through to the atomic update below.
            }
        }

        // Document already exists: apply the bit atomically.
        string maskField = isVic ? "vicMask" : "derMask";
        UpdateDefinition<HabitMonth> update = Builders<HabitMonth>.Update
            .BitwiseOr(maskField, 1L << bitIndex)
            .Set($"completionTimes.{bitIndex}", unixTimestamp);

        await context.HabitMonths.UpdateOneAsync(filter, update, null, ct);
    }
}
