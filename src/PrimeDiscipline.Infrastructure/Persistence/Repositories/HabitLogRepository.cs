using MongoDB.Driver;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Enums;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Infrastructure.Persistence.Repositories;

public sealed class HabitLogRepository(MongoDbContext context) : IHabitLogRepository
{
    public async Task<HabitLog?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await context.HabitLogs.Find(l => l.Id == id).FirstOrDefaultAsync(ct);

    public async Task<HabitLog?> GetByHabitAndDateAsync(string habitId, DateTime date, CancellationToken ct = default) =>
        await context.HabitLogs
            .Find(l => l.HabitId == habitId && l.Date == date.Date)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<HabitLog>> GetByHabitIdAsync(string habitId, CancellationToken ct = default) =>
        await context.HabitLogs.Find(l => l.HabitId == habitId)
            .SortByDescending(l => l.Date)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HabitLog>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await context.HabitLogs.Find(l => l.UserId == userId)
            .SortByDescending(l => l.Date)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<HabitLog> Items, long Total)> GetPagedByHabitIdAsync(
        string habitId, int skip, int take, CancellationToken ct = default)
    {
        FilterDefinition<HabitLog> filter = Builders<HabitLog>.Filter.Eq(l => l.HabitId, habitId);
        long total = await context.HabitLogs.CountDocumentsAsync(filter, cancellationToken: ct);
        List<HabitLog> items = await context.HabitLogs
            .Find(filter)
            .SortByDescending(l => l.Date)
            .Skip(skip)
            .Limit(take)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<HabitLog>> GetByHabitIdsAndDateAsync(
        IEnumerable<string> habitIds, DateTime date, CancellationToken ct = default)
    {
        List<string> idList = habitIds.ToList();
        if (idList.Count == 0)
            return [];

        DateTime dateOnly = date.Date;
        FilterDefinition<HabitLog> filter = Builders<HabitLog>.Filter.And(
            Builders<HabitLog>.Filter.In(l => l.HabitId, idList),
            Builders<HabitLog>.Filter.Eq(l => l.Date, dateOnly));

        return await context.HabitLogs.Find(filter).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HabitLog>> GetByHabitIdsAndDateRangeAsync(
        IEnumerable<string> habitIds, DateTime from, DateTime to, CancellationToken ct = default)
    {
        List<string> idList = habitIds.ToList();
        if (idList.Count == 0)
            return [];

        DateTime fromDate = from.Date;
        DateTime toDate   = to.Date;

        FilterDefinition<HabitLog> filter = Builders<HabitLog>.Filter.And(
            Builders<HabitLog>.Filter.In(l => l.HabitId, idList),
            Builders<HabitLog>.Filter.Gte(l => l.Date, fromDate),
            Builders<HabitLog>.Filter.Lte(l => l.Date, toDate));

        return await context.HabitLogs.Find(filter).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HabitLog>> GetByUserIdAndDateAsync(
        string userId, DateTime date, CancellationToken ct = default)
    {
        DateTime dateOnly = date.Date;
        FilterDefinition<HabitLog> filter = Builders<HabitLog>.Filter.And(
            Builders<HabitLog>.Filter.Eq(l => l.UserId, userId),
            Builders<HabitLog>.Filter.Eq(l => l.Date, dateOnly));
        return await context.HabitLogs.Find(filter).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HabitLog>> GetPendingForDateAsync(DateTime date, CancellationToken ct = default) =>
        await context.HabitLogs
            .Find(l => l.Date == date.Date && l.Status != HabitLogStatus.VIC)
            .ToListAsync(ct);

    public async Task<HabitLog> CreateAsync(HabitLog log, CancellationToken ct = default)
    {
        try
        {
            await context.HabitLogs.InsertOneAsync(log, cancellationToken: ct);
            return log;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException(
                $"A log already exists for habit '{log.HabitId}' on {log.Date:yyyy-MM-dd}.", ex);
        }
    }

    public async Task<bool> UpdateStatusAsync(string id, HabitLogStatus status, CancellationToken ct = default)
    {
        UpdateResult result = await context.HabitLogs.UpdateOneAsync(
            l => l.Id == id,
            Builders<HabitLog>.Update.Set(l => l.Status, status),
            cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task DeleteByHabitIdAsync(string habitId, CancellationToken ct = default) =>
        await context.HabitLogs.DeleteManyAsync(l => l.HabitId == habitId, ct);
}
