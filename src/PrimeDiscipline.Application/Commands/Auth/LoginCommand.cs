using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Commands.Auth;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<SessionDto>>;
