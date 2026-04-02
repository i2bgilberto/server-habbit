using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.WebAPI.Middleware;

namespace PrimeDiscipline.WebAPI.Extensions;

public static class HttpContextExtensions
{
    /// <summary>Returns the authenticated session injected by SessionAuthMiddleware.</summary>
    public static Session GetSession(this HttpContext context) =>
        (Session)context.Items[SessionAuthMiddleware.SessionKey]!;
}
