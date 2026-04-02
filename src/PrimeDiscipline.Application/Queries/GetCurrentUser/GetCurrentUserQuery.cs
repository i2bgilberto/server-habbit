using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(string UserId) : IRequest<Result<UserDto>>;
