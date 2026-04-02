using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Queries.GetUserHabits;

public sealed class GetUserHabitsQueryHandler(
    IHabitRepository habitRepository,
    IUserRepository userRepository,
    IHabitLogRepository habitLogRepository)
    : IRequestHandler<GetUserHabitsQuery, Result<IReadOnlyList<HabitDto>>>
{
    public async Task<Result<IReadOnlyList<HabitDto>>> Handle(
        GetUserHabitsQuery request, CancellationToken cancellationToken)
    {
        if (!ObjectIdGuard.IsValid(request.UserId))
            return Result.Failure<IReadOnlyList<HabitDto>>(
                Error.Validation(nameof(request.UserId), "UserId must be a valid 24-character hex ObjectId."));

        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<IReadOnlyList<HabitDto>>(Error.NotFound(nameof(User), request.UserId));

        IReadOnlyList<Habit> habits = await habitRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (habits.Count == 0)
            return Result.Success<IReadOnlyList<HabitDto>>([]);

        // Batch-load today's logs in one query — no N+1
        DateTime utcNow   = DateTime.UtcNow;
        DateTime todayUtc = utcNow.Date;

        IReadOnlyList<HabitLog> todayLogs = await habitLogRepository.GetByHabitIdsAndDateAsync(
            habits.Select(h => h.Id), todayUtc, cancellationToken);

        Dictionary<string, HabitLog> logByHabitId = todayLogs.ToDictionary(l => l.HabitId);

        IReadOnlyList<HabitDto> dtos = habits.Select(h =>
        {
            logByHabitId.TryGetValue(h.Id, out HabitLog? todayLog);
            string todayStatus = HabitStatusComputer.Compute(h, todayLog, utcNow, user.Timezone);

            HabitLogDto? todayLogDto = todayLog is null ? null : new HabitLogDto(
                todayLog.Id, todayLog.HabitId, todayLog.UserId, todayLog.Date,
                todayLog.Status, todayLog.RecordedAtUtc, todayLog.Notes);

            return new HabitDto(
                h.Id, h.UserId, h.Name, h.Description, h.TargetTime, h.WindowMinutes, h.Type,
                new HabitFrequencyDto(h.Frequency.Type, h.Frequency.DaysOfWeek, h.Frequency.TimesPerPeriod),
                h.IsActive, h.CreatedAtUtc, todayStatus, todayLogDto);
        }).ToList();

        return Result.Success(dtos);
    }
}
