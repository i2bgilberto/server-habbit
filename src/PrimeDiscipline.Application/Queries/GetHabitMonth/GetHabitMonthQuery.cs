using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Queries.GetHabitMonth;

public sealed record GetHabitMonthQuery(
    string HabitId,
    string RequestingUserId,
    int Year,
    int Month) : IRequest<Result<HabitMonthDto>>;
