namespace PrimeDiscipline.Infrastructure.Persistence.Settings;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName     { get; set; } = "PrimeDiscipline";

    // Collection names
    public string UsersCollection     { get; set; } = "users";
    public string HabitsCollection    { get; set; } = "habits";
    public string HabitLogsCollection  { get; set; } = "habitLogs";
    public string SessionsCollection   { get; set; } = "sessions";
    public string HabitMonthsCollection { get; set; } = "habitMonths";
}
