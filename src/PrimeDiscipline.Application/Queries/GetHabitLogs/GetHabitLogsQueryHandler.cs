using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Queries.GetHabitLogs;

public sealed class GetHabitLogsQueryHandler(
    IHabitRepository habitRepository,
    IHabitLogRepository habitLogRepository)
    : IRequestHandler<GetHabitLogsQuery, Result<PagedResultDto<HabitLogDto>>>
{
    public async Task<Result<PagedResultDto<HabitLogDto>>> Handle(
        GetHabitLogsQuery request, CancellationToken cancellationToken)
    {
        if (!ObjectIdGuard.IsValid(request.HabitId))
            return Result.Failure<PagedResultDto<HabitLogDto>>(
                Error.Validation(nameof(request.HabitId), "HabitId must be a valid 24-character hex ObjectId."));

        if (!ObjectIdGuard.IsValid(request.RequestingUserId))
            return Result.Failure<PagedResultDto<HabitLogDto>>(
                Error.Validation(nameof(request.RequestingUserId), "RequestingUserId must be a valid 24-character hex ObjectId."));

        Habit? habit = await habitRepository.GetByIdAsync(request.HabitId, cancellationToken);
        if (habit is null)
            return Result.Failure<PagedResultDto<HabitLogDto>>(
                Error.NotFound(nameof(Habit), request.HabitId));

        if (habit.UserId != request.RequestingUserId)
            return Result.Failure<PagedResultDto<HabitLogDto>>(
                Error.Forbidden("You do not own this habit."));

        int page     = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, 100);
        int skip     = (page - 1) * pageSize;

        (IReadOnlyList<HabitLog> logs, long total) = await habitLogRepository.GetPagedByHabitIdAsync(
            request.HabitId, skip, pageSize, cancellationToken);

        IReadOnlyList<HabitLogDto> items = logs
            .Select(l => new HabitLogDto(l.Id, l.HabitId, l.UserId, l.Date, l.Status, l.RecordedAtUtc, l.Notes))
            .ToList();

        return Result.Success(new PagedResultDto<HabitLogDto>(items, page, pageSize, (int)total));
    }
}
