using MediatR;
using PrimeDiscipline.Application.Common;

namespace PrimeDiscipline.Application.Commands.DeleteHabit;

public sealed record DeleteHabitCommand(
    string HabitId,
    string RequestingUserId) : IRequest<Result<bool>>;
