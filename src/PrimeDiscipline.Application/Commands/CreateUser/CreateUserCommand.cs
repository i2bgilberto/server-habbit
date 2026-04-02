using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string DisplayName,
    string Timezone) : IRequest<Result<UserDto>>;
