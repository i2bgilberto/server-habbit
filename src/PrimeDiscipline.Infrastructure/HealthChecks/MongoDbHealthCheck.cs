using Microsoft.Extensions.Diagnostics.HealthChecks;
using PrimeDiscipline.Infrastructure.Persistence;

namespace PrimeDiscipline.Infrastructure.HealthChecks;

public sealed class MongoDbHealthCheck(MongoDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext ctx, CancellationToken ct = default)
    {
        try
        {
            await context.Users.CountDocumentsAsync(
                MongoDB.Driver.Builders<PrimeDiscipline.Domain.Entities.User>.Filter.Empty, cancellationToken: ct);
            return HealthCheckResult.Healthy("MongoDB reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB unreachable.", ex);
        }
    }
}
