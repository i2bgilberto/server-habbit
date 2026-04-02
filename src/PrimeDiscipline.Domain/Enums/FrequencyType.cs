namespace PrimeDiscipline.Domain.Enums;

public enum FrequencyType
{
    Daily         = 0,  // every day
    Weekdays      = 1,  // Mon–Fri only
    SixDaysAWeek  = 2,  // 6 fixed days, user picks rest day (stored in DaysOfWeek)
    EveryOtherDay = 3,  // alternating from habit creation date
    TimesPerWeek  = 4,  // N times any day of the week (stored in TimesPerPeriod)
    Custom        = 5,  // specific days of week (stored in DaysOfWeek)
    TimesPerMonth = 6   // N times any day of the month (stored in TimesPerPeriod)
}
