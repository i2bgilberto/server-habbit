namespace PrimeDiscipline.Application.DTOs;

public sealed record SessionDto(
    string UserId,
    string DisplayName,
    string Email,
    string Token,
    DateTime ExpiresAtUtc);
