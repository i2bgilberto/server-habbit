using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PrimeDiscipline.Application;
using PrimeDiscipline.Infrastructure;
using PrimeDiscipline.Infrastructure.HealthChecks;
using PrimeDiscipline.Infrastructure.Persistence;
using PrimeDiscipline.WebAPI.Middleware;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ── Windows Service support ──────────────────────────────────────────────────
builder.Host.UseWindowsService();

// ── Kestrel hardening ────────────────────────────────────────────────────────
builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.AddServerHeader = false;
    kestrel.Limits.MaxRequestBodySize = 1_048_576; // 1 MB
});

// ── MONGODB_URI env var overrides appsettings ────────────────────────────────
string? mongoUri = Environment.GetEnvironmentVariable("MONGODB_URI");
if (!string.IsNullOrWhiteSpace(mongoUri))
    builder.Configuration["MongoDb:ConnectionString"] = mongoUri;

// ── Application & Infrastructure ────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── CORS ─────────────────────────────────────────────────────────────────────
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ── Rate Limiting ─────────────────────────────────────────────────────────────
int loginPermit  = builder.Configuration.GetValue<int>("RateLimit:LoginPermitLimit", 5);
int loginWindowS = builder.Configuration.GetValue<int>("RateLimit:LoginWindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", cfg =>
    {
        cfg.PermitLimit         = loginPermit;
        cfg.Window              = TimeSpan.FromSeconds(loginWindowS);
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit          = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, _) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsJsonAsync(new
        {
            code    = "TOO_MANY_REQUESTS",
            message = $"Too many login attempts. Try again in {loginWindowS} seconds.",
        });
    };
});

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb");

// ── OpenAPI (dev only) ────────────────────────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title       = "Prime Discipline API";
        document.Info.Version     = "v1";
        document.Info.Description =
            "Habit management API with strict server-side time-window validation. " +
            "Auth: send X-Session-Token header. Status codes: VIC=0, DER=1, MISS=2.";
        return Task.CompletedTask;
    });
});

// ── Pipeline ──────────────────────────────────────────────────────────────────
WebApplication app = builder.Build();

// Ensure MongoDB indexes on startup
using (IServiceScope scope = app.Services.CreateScope())
{
    MongoDbContext context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await context.EnsureIndexesAsync();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseMiddleware<SessionAuthMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Prime Discipline";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
