using MongoDB.Driver;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(MongoDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await context.Users.Find(u => u.Id == id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<string, User>> GetByIdsAsync(
        IEnumerable<string> ids, CancellationToken ct = default)
    {
        List<string> idList = ids.ToList();
        if (idList.Count == 0)
            return new Dictionary<string, User>();

        FilterDefinition<User> filter = Builders<User>.Filter.In(u => u.Id, idList);
        List<User> users = await context.Users.Find(filter).ToListAsync(ct);
        return users.ToDictionary(u => u.Id);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Users.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await context.Users.Find(_ => true).ToListAsync(ct);

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        await context.Users.InsertOneAsync(user, cancellationToken: ct);
        return user;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        ReplaceOneResult result = await context.Users.ReplaceOneAsync(
            u => u.Id == user.Id, user, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        long count = await context.Users
            .CountDocumentsAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken: ct);
        return count > 0;
    }
}
