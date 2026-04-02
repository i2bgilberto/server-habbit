using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Commands.RecordActivity;

public sealed record RecordActivityCommand(
    string HabitId,
    string UserId,
    string Notes) : IRequest<Result<HabitLogDto>>;
