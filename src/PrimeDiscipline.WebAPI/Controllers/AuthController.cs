using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PrimeDiscipline.Application.Commands.Auth;
using PrimeDiscipline.Application.DTOs;
using PrimeDiscipline.Application.Queries.GetCurrentUser;
using PrimeDiscipline.WebAPI.Extensions;
using PrimeDiscipline.WebAPI.Filters;

namespace PrimeDiscipline.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>Creates a new account (Argon2id password hash).</summary>
    [HttpPost("register")]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        Application.Common.Result<UserDto> result = await mediator.Send(command, ct);
        return result.ToHttpResult(StatusCodes.Status201Created);
    }

    /// <summary>Validates credentials and returns an opaque session token.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        Application.Common.Result<SessionDto> result = await mediator.Send(command, ct);
        return result.ToHttpResult();
    }

    /// <summary>Revokes the current session token.</summary>
    [HttpPost("logout")]
    [RequireAuth]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Logout(CancellationToken ct)
    {
        string token = HttpContext.GetSession().Token;
        Application.Common.Result<bool> result = await mediator.Send(new LogoutCommand(token), ct);
        return result.ToHttpResult();
    }

    /// <summary>Returns the profile of the authenticated user.</summary>
    [HttpGet("me")]
    [RequireAuth]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Me(CancellationToken ct)
    {
        string userId = HttpContext.GetSession().UserId;
        Application.Common.Result<UserDto> result = await mediator.Send(new GetCurrentUserQuery(userId), ct);
        return result.ToHttpResult();
    }
}
