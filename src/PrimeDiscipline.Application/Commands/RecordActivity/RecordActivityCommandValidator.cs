using FluentValidation;

namespace PrimeDiscipline.Application.Commands.RecordActivity;

public sealed class RecordActivityCommandValidator : AbstractValidator<RecordActivityCommand>
{
    public RecordActivityCommandValidator()
    {
        RuleFor(x => x.HabitId)
            .NotEmpty().WithMessage("HabitId is required.")
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("HabitId must be a valid MongoDB ObjectId.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("UserId must be a valid MongoDB ObjectId.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.");
    }
}
