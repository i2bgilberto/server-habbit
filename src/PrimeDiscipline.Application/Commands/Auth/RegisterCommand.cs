using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Commands.Auth;

public sealed record RegisterCommand(
    string Email,
    string DisplayName,
    string Timezone,
    string Password) : IRequest<Result<UserDto>>;
