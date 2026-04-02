namespace PrimeDiscipline.Application.DTOs;

public sealed record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string Timezone,
    DateTime CreatedAtUtc);
