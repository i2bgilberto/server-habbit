using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.WebAPI.Middleware;

namespace PrimeDiscipline.WebAPI.Filters;

/// <summary>
/// Apply to any controller or action that requires a valid session.
/// Returns 401 when X-Session-Token is missing or expired.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAuthAttribute : Attribute, IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        Session? session = context.HttpContext.Items[SessionAuthMiddleware.SessionKey] as Session;
        if (session is null)
        {
            context.Result = new JsonResult(new { code = "UNAUTHORIZED", message = "Valid X-Session-Token header is required." })
            {
                StatusCode = StatusCodes.Status401Unauthorized,
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
