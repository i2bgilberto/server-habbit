using MongoDB.Driver;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository(MongoDbContext context) : ISessionRepository
{
    public async Task<Session> CreateAsync(Session session, CancellationToken ct = default)
    {
        await context.Sessions.InsertOneAsync(session, cancellationToken: ct);
        return session;
    }

    public async Task<Session?> GetByTokenAsync(string token, CancellationToken ct = default) =>
        await context.Sessions
            .Find(s => s.Token == token && !s.IsRevoked && s.ExpiresAtUtc > DateTime.UtcNow)
            .FirstOrDefaultAsync(ct);

    public async Task<bool> RevokeAsync(string token, CancellationToken ct = default)
    {
        UpdateResult result = await context.Sessions.UpdateOneAsync(
            s => s.Token == token,
            Builders<Session>.Update.Set(s => s.IsRevoked, true),
            cancellationToken: ct);
        return result.ModifiedCount > 0;
    }
}
