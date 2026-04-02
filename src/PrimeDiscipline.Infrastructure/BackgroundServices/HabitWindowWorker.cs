using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrimeDiscipline.Application.Commands.RecordActivity;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Enums;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Infrastructure.BackgroundServices;

/// <summary>
/// Runs every minute:
/// 1. Auto-DER — marks habits as DER when their time window expired and no log exists yet.
/// 2. Midnight sweep — marks habits as MISS when a full day passed without any log.
///
/// Frequency rules (all types: day without log = MISS):
///   Daily         → every day
///   Weekdays      → Mon–Fri only
///   SixDaysAWeek  → the 6 days stored in DaysOfWeek
///   EveryOtherDay → alternating from habit creation date
///   Custom        → the days stored in DaysOfWeek
///   TimesPerWeek  → every day (quota is a progress metric, not a schedule filter)
///   TimesPerMonth → every day (same reasoning)
/// </summary>
public sealed class HabitWindowWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<HabitWindowWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("HabitWindowWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTickAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error in HabitWindowWorker tick.");
            }

            await Task.Delay(TickInterval, stoppingToken);
        }
    }

    private async Task ProcessTickAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        IHabitRepository    habitRepo    = scope.ServiceProvider.GetRequiredService<IHabitRepository>();
        IHabitLogRepository habitLogRepo = scope.ServiceProvider.GetRequiredService<IHabitLogRepository>();
        IUserRepository     userRepo     = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        DateTime utcNow   = DateTime.UtcNow;
        DateTime todayUtc = utcNow.Date;

        IReadOnlyList<Habit> activeHabits = await habitRepo.GetAllActiveAsync(ct);
        if (activeHabits.Count == 0)
            return;

        // Batch-load today's logs and all relevant users
        IReadOnlyList<string> habitIds = activeHabits.Select(h => h.Id).ToList();
        IReadOnlyList<HabitLog> todaysLogs = await habitLogRepo.GetByHabitIdsAndDateAsync(habitIds, todayUtc, ct);
        HashSet<string> loggedToday = todaysLogs.Select(l => l.HabitId).ToHashSet();

        IReadOnlyList<string> userIds = activeHabits.Select(h => h.UserId).Distinct().ToList();
        IReadOnlyDictionary<string, User> usersById = await userRepo.GetByIdsAsync(userIds, ct);

        // ── Auto-DER sweep ────────────────────────────────────────────────────
        foreach (Habit habit in activeHabits)
        {
            if (loggedToday.Contains(habit.Id))
                continue;

            if (!usersById.TryGetValue(habit.UserId, out User? user))
                continue;

            if (!IsScheduledForDate(habit, todayUtc))
                continue;

            TimeZoneInfo tz;
            try { tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone); }
            catch (TimeZoneNotFoundException)
            {
                logger.LogWarning("Unknown timezone '{Tz}' for user {UserId}.", user.Timezone, user.Id);
                continue;
            }

            TimeSpan localTimeOfDay = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz).TimeOfDay;

            if (localTimeOfDay > habit.WindowEnd)
            {
                HabitLog derLog = HabitLogFactory.CreateAutoDefeat(habit.Id, habit.UserId, todayUtc);
                await habitLogRepo.CreateAsync(derLog, ct);
                logger.LogInformation("Auto-DER for Habit {HabitId} (user {UserId}).", habit.Id, habit.UserId);
            }
        }

        await CloseYesterdayMissesAsync(activeHabits, habitLogRepo, todayUtc, ct);
    }

    /// <summary>
    /// Runs only in the first UTC minute after midnight.
    /// Creates a MISS for every habit that was scheduled yesterday but has no log.
    /// </summary>
    private async Task CloseYesterdayMissesAsync(
        IReadOnlyList<Habit> activeHabits,
        IHabitLogRepository habitLogRepo,
        DateTime todayUtc,
        CancellationToken ct)
    {
        if (DateTime.UtcNow.TimeOfDay > TimeSpan.FromMinutes(1))
            return;

        DateTime yesterdayUtc = todayUtc.AddDays(-1);

        IReadOnlyList<string> habitIds = activeHabits.Select(h => h.Id).ToList();
        IReadOnlyList<HabitLog> yesterdaysLogs =
            await habitLogRepo.GetByHabitIdsAndDateAsync(habitIds, yesterdayUtc, ct);
        HashSet<string> loggedYesterday = yesterdaysLogs.Select(l => l.HabitId).ToHashSet();

        foreach (Habit habit in activeHabits)
        {
            if (!IsScheduledForDate(habit, yesterdayUtc))
                continue;

            if (!loggedYesterday.Contains(habit.Id))
            {
                HabitLog missLog = HabitLogFactory.CreateMiss(habit.Id, habit.UserId, yesterdayUtc);
                await habitLogRepo.CreateAsync(missLog, ct);
                logger.LogInformation(
                    "MISS for Habit {HabitId} (user {UserId}) on {Date:yyyy-MM-dd}.",
                    habit.Id, habit.UserId, yesterdayUtc);
            }
        }
    }

    /// <summary>
    /// Returns true if the worker should auto-close this habit on the given date.
    /// The creation day is always excluded — tracking starts the day after creation.
    /// TimesPerWeek and TimesPerMonth are scheduled every day — the quota is a progress metric only.
    /// </summary>
    private static bool IsScheduledForDate(Habit habit, DateTime date)
    {
        // Creation day: user may log manually but the worker never auto-closes it
        if (date.Date == habit.CreatedAtUtc.Date)
            return false;

        return habit.Frequency.Type switch
        {
            FrequencyType.Daily         => true,
            FrequencyType.TimesPerWeek  => true,
            FrequencyType.TimesPerMonth => true,
            FrequencyType.Weekdays      => date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday,
            FrequencyType.SixDaysAWeek  => habit.Frequency.DaysOfWeek?.Contains((int)date.DayOfWeek) == true,
            FrequencyType.Custom        => habit.Frequency.DaysOfWeek?.Contains((int)date.DayOfWeek) == true,
            FrequencyType.EveryOtherDay => (int)(date.Date - habit.CreatedAtUtc.Date).TotalDays % 2 == 0,
            _                           => true,
        };
    }
}
