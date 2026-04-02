using MediatR;
using Microsoft.AspNetCore.Mvc;
using PrimeDiscipline.Application.Commands.RecordActivity;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Application.Queries.GetHabitLogs;
using PrimeDiscipline.WebAPI.Extensions;
using PrimeDiscipline.WebAPI.Filters;

namespace PrimeDiscipline.WebAPI.Controllers;

[ApiController]
[Route("api/habit-logs")]
[Produces("application/json")]
[RequireAuth]
public sealed class HabitLogsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Records a habit activity attempt. Server validates the time window using DateTime.UtcNow.
    /// Returns VIC if within the window, DER otherwise.
    /// UserId is taken from the session token.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<HabitLogDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IResult> RecordActivity([FromBody] RecordActivityRequest body, CancellationToken ct)
    {
        string userId = HttpContext.GetSession().UserId;
        RecordActivityCommand command = new(body.HabitId, userId, body.Notes);
        Application.Common.Result<HabitLogDto> result = await mediator.Send(command, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    /// <summary>Returns paginated log entries for a habit owned by the authenticated user.</summary>
    [HttpGet("{habitId}")]
    [ProducesResponseType<PagedResultDto<HabitLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetHabitLogs(
        [FromRoute] string habitId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct     = default)
    {
        string userId = HttpContext.GetSession().UserId;
        Application.Common.Result<PagedResultDto<HabitLogDto>> result =
            await mediator.Send(new GetHabitLogsQuery(habitId, userId, page, pageSize), ct);
        return result.ToHttpResult();
    }
}

public sealed record RecordActivityRequest(string HabitId, string Notes);
