using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Queries.GetTodayLogs;

public sealed record GetTodayLogsQuery(string UserId) : IRequest<Result<IReadOnlyList<HabitLogDto>>>;
