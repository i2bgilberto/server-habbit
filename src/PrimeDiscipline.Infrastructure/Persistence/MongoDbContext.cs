using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Infrastructure.Persistence.Settings;

namespace PrimeDiscipline.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;
    private readonly ILogger<MongoDbContext> _logger;

    public MongoDbContext(IOptions<MongoDbSettings> options, ILogger<MongoDbContext> logger)
    {
        _settings = options.Value;
        _logger   = logger;

        MongoClient client = new(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<User>     Users     => _database.GetCollection<User>(_settings.UsersCollection);
    public IMongoCollection<Habit>    Habits    => _database.GetCollection<Habit>(_settings.HabitsCollection);
    public IMongoCollection<HabitLog> HabitLogs => _database.GetCollection<HabitLog>(_settings.HabitLogsCollection);
    public IMongoCollection<Session>  Sessions  => _database.GetCollection<Session>(_settings.SessionsCollection);

    public async Task EnsureIndexesAsync(CancellationToken ct = default)
    {
        await EnsureUserIndexesAsync(ct);
        await EnsureHabitIndexesAsync(ct);
        await EnsureHabitLogIndexesAsync(ct);
        await EnsureSessionIndexesAsync(ct);
        _logger.LogInformation("MongoDB indexes verified.");
    }

    private async Task EnsureUserIndexesAsync(CancellationToken ct)
    {
        IndexKeysDefinition<User> emailKey = Builders<User>.IndexKeys.Ascending(u => u.Email);
        CreateIndexModel<User> emailIndex  = new(emailKey, new CreateIndexOptions { Unique = true, Name = "idx_users_email" });
        await Users.Indexes.CreateOneAsync(emailIndex, cancellationToken: ct);
    }

    private async Task EnsureHabitIndexesAsync(CancellationToken ct)
    {
        IndexKeysDefinition<Habit> userKey = Builders<Habit>.IndexKeys.Ascending(h => h.UserId);
        CreateIndexModel<Habit> userIndex  = new(userKey, new CreateIndexOptions { Name = "idx_habits_userId" });
        await Habits.Indexes.CreateOneAsync(userIndex, cancellationToken: ct);

        // HabitWindowWorker runs every minute filtering by isActive — needs an index
        IndexKeysDefinition<Habit> activeKey = Builders<Habit>.IndexKeys.Ascending(h => h.IsActive);
        CreateIndexModel<Habit> activeIndex  = new(activeKey, new CreateIndexOptions { Name = "idx_habits_isActive" });
        await Habits.Indexes.CreateOneAsync(activeIndex, cancellationToken: ct);
    }

    private async Task EnsureHabitLogIndexesAsync(CancellationToken ct)
    {
        IndexKeysDefinition<HabitLog> compoundKey = Builders<HabitLog>.IndexKeys
            .Ascending(l => l.HabitId)
            .Ascending(l => l.Date);

        CreateIndexModel<HabitLog> compoundIndex = new(
            compoundKey,
            new CreateIndexOptions { Unique = true, Name = "idx_habitLogs_habitId_date" });

        await HabitLogs.Indexes.CreateOneAsync(compoundIndex, cancellationToken: ct);

        IndexKeysDefinition<HabitLog> dateStatusKey = Builders<HabitLog>.IndexKeys
            .Ascending(l => l.Date)
            .Ascending(l => l.Status);

        CreateIndexModel<HabitLog> dateStatusIndex = new(
            dateStatusKey,
            new CreateIndexOptions { Name = "idx_habitLogs_date_status" });

        await HabitLogs.Indexes.CreateOneAsync(dateStatusIndex, cancellationToken: ct);

        // GetByUserIdAsync needs a userId index
        IndexKeysDefinition<HabitLog> userIdKey = Builders<HabitLog>.IndexKeys.Ascending(l => l.UserId);
        CreateIndexModel<HabitLog> userIdIndex   = new(userIdKey, new CreateIndexOptions { Name = "idx_habitLogs_userId" });
        await HabitLogs.Indexes.CreateOneAsync(userIdIndex, cancellationToken: ct);
    }

    private async Task EnsureSessionIndexesAsync(CancellationToken ct)
    {
        // Unique token lookup
        IndexKeysDefinition<Session> tokenKey = Builders<Session>.IndexKeys.Ascending(s => s.Token);
        CreateIndexModel<Session> tokenIndex  = new(tokenKey, new CreateIndexOptions { Unique = true, Name = "idx_sessions_token" });
        await Sessions.Indexes.CreateOneAsync(tokenIndex, cancellationToken: ct);

        // TTL index: MongoDB auto-deletes expired sessions after 1 hour of expiry
        IndexKeysDefinition<Session> ttlKey = Builders<Session>.IndexKeys.Ascending(s => s.ExpiresAtUtc);
        CreateIndexModel<Session> ttlIndex  = new(ttlKey, new CreateIndexOptions { ExpireAfter = TimeSpan.FromHours(1), Name = "idx_sessions_ttl" });
        await Sessions.Indexes.CreateOneAsync(ttlIndex, cancellationToken: ct);
    }
}
