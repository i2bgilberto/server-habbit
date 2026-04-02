using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Queries.GetHabitMonthsBatch;

public sealed record GetHabitMonthsBatchQuery(
    string RequestingUserId,
    int Year,
    int Month) : IRequest<Result<IReadOnlyList<HabitMonthDto>>>;
