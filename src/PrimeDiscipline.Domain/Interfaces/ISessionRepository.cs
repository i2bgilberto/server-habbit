using PrimeDiscipline.Domain.Entities;

namespace PrimeDiscipline.Domain.Interfaces;

public interface ISessionRepository
{
    Task<Session> CreateAsync(Session session, CancellationToken ct = default);
    Task<Session?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<bool> RevokeAsync(string token, CancellationToken ct = default);
}
