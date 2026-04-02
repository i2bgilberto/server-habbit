using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Entities;
using PrimeDiscipline.Domain.Interfaces;

namespace PrimeDiscipline.Application.Queries.GetTodayLogs;

public sealed class GetTodayLogsQueryHandler(IHabitLogRepository habitLogRepository)
    : IRequestHandler<GetTodayLogsQuery, Result<IReadOnlyList<HabitLogDto>>>
{
    public async Task<Result<IReadOnlyList<HabitLogDto>>> Handle(
        GetTodayLogsQuery request, CancellationToken cancellationToken)
    {
        DateTime todayUtc = DateTime.UtcNow.Date;

        IReadOnlyList<HabitLog> logs = await habitLogRepository.GetByUserIdAndDateAsync(
            request.UserId, todayUtc, cancellationToken);

        IReadOnlyList<HabitLogDto> dtos = logs
            .Select(l => new HabitLogDto(l.Id, l.HabitId, l.UserId, l.Date, l.Status, l.RecordedAtUtc, l.Notes))
            .ToList();

        return Result.Success(dtos);
    }
}
