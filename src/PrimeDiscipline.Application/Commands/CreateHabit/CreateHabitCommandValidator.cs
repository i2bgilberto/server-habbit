using FluentValidation;
using PrimeDiscipline.Domain.Enums;

namespace PrimeDiscipline.Application.Commands.CreateHabit;

public sealed class CreateHabitCommandValidator : AbstractValidator<CreateHabitCommand>
{
    public CreateHabitCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .Length(24)
            .Matches("^[a-fA-F0-9]{24}$").WithMessage("UserId must be a valid MongoDB ObjectId.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.TargetTime)
            .NotEmpty()
            .Matches(@"^\d{2}:\d{2}(:\d{2})?$").WithMessage("TargetTime must be in HH:mm or HH:mm:ss format.")
            .Must(BeValidTimeSpan).WithMessage("TargetTime must represent a valid time of day.");

        RuleFor(x => x.WindowMinutes)
            .InclusiveBetween(1, 720);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Type must be a valid HabitType value.");

        RuleFor(x => x.FrequencyType)
            .IsInEnum().WithMessage("FrequencyType must be a valid value.");

        // DaysOfWeek required for SixDaysAWeek and Custom
        RuleFor(x => x.DaysOfWeek)
            .NotNull().WithMessage("DaysOfWeek is required for this frequency type.")
            .Must(d => d != null && d.Count > 0).WithMessage("DaysOfWeek must have at least one day.")
            .Must(d => d == null || d.All(v => v is >= 0 and <= 6))
                .WithMessage("DaysOfWeek values must be 0 (Sun) to 6 (Sat).")
            .When(x => x.FrequencyType is FrequencyType.SixDaysAWeek or FrequencyType.Custom);

        RuleFor(x => x.DaysOfWeek)
            .Must(d => d == null || d.Count == 6).WithMessage("SixDaysAWeek requires exactly 6 days.")
            .When(x => x.FrequencyType == FrequencyType.SixDaysAWeek);

        // TimesPerPeriod required for TimesPerWeek and TimesPerMonth
        RuleFor(x => x.TimesPerPeriod)
            .NotNull().WithMessage("TimesPerPeriod is required for this frequency type.")
            .When(x => x.FrequencyType is FrequencyType.TimesPerWeek or FrequencyType.TimesPerMonth);

        RuleFor(x => x.TimesPerPeriod)
            .InclusiveBetween(1, 7).WithMessage("TimesPerWeek must be between 1 and 7.")
            .When(x => x.FrequencyType == FrequencyType.TimesPerWeek && x.TimesPerPeriod.HasValue);

        RuleFor(x => x.TimesPerPeriod)
            .InclusiveBetween(1, 31).WithMessage("TimesPerMonth must be between 1 and 31.")
            .When(x => x.FrequencyType == FrequencyType.TimesPerMonth && x.TimesPerPeriod.HasValue);
    }

    private static bool BeValidTimeSpan(string value)
    {
        if (!TimeSpan.TryParse(value, out TimeSpan ts)) return false;
        return ts >= TimeSpan.Zero && ts < TimeSpan.FromHours(24);
    }
}
