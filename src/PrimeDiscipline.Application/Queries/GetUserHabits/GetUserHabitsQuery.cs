using MediatR;
using PrimeDiscipline.Application.Common;
using PrimeDiscipline.Application.DTOs;

namespace PrimeDiscipline.Application.Queries.GetUserHabits;

public sealed record GetUserHabitsQuery(string UserId) : IRequest<Result<IReadOnlyList<HabitDto>>>;
