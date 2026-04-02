using MediatR;
using Microsoft.AspNetCore.Mvc;
using PrimeDiscipline.Application.Commands.CreateUser;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Application.Queries.GetUserHabits;
using PrimeDiscipline.WebAPI.Extensions;
using PrimeDiscipline.WebAPI.Filters;

namespace PrimeDiscipline.WebAPI.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a user without password (legacy/admin use). Prefer /auth/register.</summary>
    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        Application.Common.Result<UserDto> result = await mediator.Send(command, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    /// <summary>Returns all habits of the authenticated user.</summary>
    [HttpGet("me/habits")]
    [RequireAuth]
    [ProducesResponseType<IReadOnlyList<HabitDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetMyHabits(CancellationToken ct)
    {
        string userId = HttpContext.GetSession().UserId;
        Application.Common.Result<IReadOnlyList<HabitDto>> result =
            await mediator.Send(new GetUserHabitsQuery(userId), ct);
        return result.ToHttpResult();
    }

    /// <summary>Returns habits for a specific user (admin/debug).</summary>
    [HttpGet("{userId}/habits")]
    [ProducesResponseType<IReadOnlyList<HabitDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IResult> GetUserHabits([FromRoute] string userId, CancellationToken ct)
    {
        Application.Common.Result<IReadOnlyList<HabitDto>> result =
            await mediator.Send(new GetUserHabitsQuery(userId), ct);
        return result.ToHttpResult();
    }
}
