namespace PrimeDiscipline.Application.Common;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string resource, string id) =>
        new("NOT_FOUND", $"{resource} with id '{id}' was not found.");

    public static Error Conflict(string message) =>
        new("CONFLICT", message);

    public static Error Validation(string field, string message) =>
        new($"VALIDATION.{field.ToUpperInvariant()}", message);

    public static Error BusinessRule(string message) =>
        new("BUSINESS_RULE", message);

    public static Error Forbidden(string message) =>
        new("FORBIDDEN", message);
}
