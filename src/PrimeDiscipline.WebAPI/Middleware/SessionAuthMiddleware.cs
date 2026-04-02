using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.WebAPI.Middleware;

/// <summary>
/// Reads X-Session-Token header on every request.
/// If the token is valid and not expired, injects the session into HttpContext.Items
/// so downstream filters and controllers can use it without hitting the DB again.
/// </summary>
public sealed class SessionAuthMiddleware(RequestDelegate next)
{
    public const string SessionKey = "CurrentSession";

    public async Task InvokeAsync(HttpContext context)
    {
        string? token = context.Request.Headers["X-Session-Token"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(token))
        {
            ISessionRepository repo = context.RequestServices.GetRequiredService<ISessionRepository>();
            Session? session        = await repo.GetByTokenAsync(token);

            if (session is not null)
                context.Items[SessionKey] = session;
        }

        await next(context);
    }
}
