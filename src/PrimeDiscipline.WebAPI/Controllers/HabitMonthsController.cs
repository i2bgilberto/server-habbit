using MediatR;
using Microsoft.AspNetCore.Mvc;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Application.Queries.GetHabitMonth;
using PrimeDiscipline.Application.Queries.GetHabitMonthsBatch;
using PrimeDiscipline.WebAPI.Extensions;
using PrimeDiscipline.WebAPI.Filters;

namespace PrimeDiscipline.WebAPI.Controllers;

[ApiController]
[Route("api/habit-months")]
[Produces("application/json")]
[RequireAuth]
public sealed class HabitMonthsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns bitmask monthly progress for ALL habits of the authenticated user in one call.
    /// Habits with no logs yet return a computed virtual record.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<HabitMonthDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetHabitMonthsBatch(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        string userId   = HttpContext.GetSession().UserId;
        DateTime utcNow = DateTime.UtcNow;

        Application.Common.Result<IReadOnlyList<HabitMonthDto>> result = await mediator.Send(
            new GetHabitMonthsBatchQuery(
                userId,
                year  ?? utcNow.Year,
                month ?? utcNow.Month),
            ct);

        return result.ToHttpResult();
    }

    /// <summary>
    /// Returns the bitmask monthly progress for a single habit.
    /// If no record exists yet (no logs this month) returns a computed virtual record.
    /// </summary>
    [HttpGet("{habitId}")]
    [ProducesResponseType<HabitMonthDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetHabitMonth(
        [FromRoute] string habitId,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        string userId   = HttpContext.GetSession().UserId;
        DateTime utcNow = DateTime.UtcNow;

        Application.Common.Result<HabitMonthDto> result = await mediator.Send(
            new GetHabitMonthQuery(
                habitId, userId,
                year  ?? utcNow.Year,
                month ?? utcNow.Month),
            ct);

        return result.ToHttpResult();
    }
}
