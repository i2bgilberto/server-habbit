namespace PrimeDiscipline.WebAPI.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            IHeaderDictionary h = context.Response.Headers;
            h["X-Content-Type-Options"]  = "nosniff";
            h["X-Frame-Options"]         = "DENY";
            h["X-XSS-Protection"]        = "0";          // modern browsers ignore; tells old ones to disable broken XSS filter
            h["Referrer-Policy"]         = "strict-origin-when-cross-origin";
            h["Permissions-Policy"]      = "geolocation=(), microphone=(), camera=()";
            h.Remove("Server");
            return Task.CompletedTask;
        });

        await next(context);
    }
}
