namespace PrimeDiscipline.Application.DTOs;

public sealed record HabitMonthDto(
    string HabitId,
    int Year,
    int Month,
    int BitLength,
    int StartedFromDay,
    long GoalMask,
    long VicMask,
    long DerMask,
    long MissMask,           // computed: goal & ~(vic|der) & past
    long PendingMask,        // computed: goal & ~(vic|der) & future
    long[] CompletionTimes,  // unix timestamp per bit (0 = no action)
    int ExpectedCount,
    int VicCount,
    int DerCount,
    int MissCount,
    int PendingCount,
    double VicRate,          // vicCount / expectedCount
    List<int> GoalDays,
    List<int> VicDays,
    List<int> DerDays,
    List<int> MissDays,
    List<int> PendingDays
);
