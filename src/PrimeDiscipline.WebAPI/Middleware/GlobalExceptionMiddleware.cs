using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PrimeDiscipline.WebAPI.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            ProblemResponse problem = new(
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred.",
                context.Request.Path);

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem, JsonSerializerOptions.Web));
        }
    }

    private sealed record ProblemResponse(string Code, string Message, string Path);
}
