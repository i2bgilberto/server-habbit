using MediatR;
using PrimeDiscipline.Application.Common;

namespace PrimeDiscipline.Application.Commands.Auth;

public sealed record LogoutCommand(string Token) : IRequest<Result<bool>>;
