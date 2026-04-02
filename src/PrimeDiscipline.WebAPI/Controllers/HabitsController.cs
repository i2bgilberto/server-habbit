using MediatR;
using Microsoft.AspNetCore.Mvc;
using PrimeDiscipline.Application.Commands.CreateHabit;
using PrimeDiscipline.Application.Commands.UpdateHabit;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Domain.Enums;
using PrimeDiscipline.WebAPI.Extensions;
using PrimeDiscipline.WebAPI.Filters;

namespace PrimeDiscipline.WebAPI.Controllers;

[ApiController]
[Route("api/habits")]
[Produces("application/json")]
[RequireAuth]
public sealed class HabitsController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new habit. UserId is taken from the session token.</summary>
    [HttpPost]
    [ProducesResponseType<HabitDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IResult> CreateHabit([FromBody] CreateHabitRequest body, CancellationToken ct)
    {
        string userId = HttpContext.GetSession().UserId;
        CreateHabitCommand command = new(
            userId, body.Name, body.Description, body.TargetTime, body.WindowMinutes,
            body.Type, body.FrequencyType, body.DaysOfWeek, body.TimesPerPeriod);
        Application.Common.Result<HabitDto> result = await mediator.Send(command, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    /// <summary>Updates an existing habit. UserId is taken from the session token.</summary>
    [HttpPut("{habitId}")]
    [ProducesResponseType<HabitDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IResult> UpdateHabit(
        [FromRoute] string habitId,
        [FromBody] UpdateHabitRequest body,
        CancellationToken ct)
    {
        string userId = HttpContext.GetSession().UserId;
        UpdateHabitCommand command = new(
            habitId, userId, body.Name, body.Description, body.TargetTime, body.WindowMinutes,
            body.IsActive, body.FrequencyType, body.DaysOfWeek, body.TimesPerPeriod);
        Application.Common.Result<HabitDto> result = await mediator.Send(command, ct);
        return result.ToHttpResult();
    }
}

public sealed record CreateHabitRequest(
    string Name,
    string Description,
    string TargetTime,
    int WindowMinutes,
    HabitType Type,
    FrequencyType FrequencyType = FrequencyType.Daily,
    List<int>? DaysOfWeek = null,
    int? TimesPerPeriod = null);

public sealed record UpdateHabitRequest(
    string Name,
    string Description,
    string TargetTime,
    int WindowMinutes,
    bool IsActive,
    FrequencyType FrequencyType = FrequencyType.Daily,
    List<int>? DaysOfWeek = null,
    int? TimesPerPeriod = null);
