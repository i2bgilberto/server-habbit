using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Queries.GetHabitLogs;

public sealed record GetHabitLogsQuery(
    string HabitId,
    string RequestingUserId,
    int Page     = 1,
    int PageSize = 20) : IRequest<Result<PagedResultDto<HabitLogDto>>>;
