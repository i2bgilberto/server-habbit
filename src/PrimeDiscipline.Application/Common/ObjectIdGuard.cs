using System.Text.RegularExpressions;

namespace PrimeDiscipline.Application.Common;

public static class ObjectIdGuard
{
    private static readonly Regex Pattern = new("^[a-fA-F0-9]{24}$", RegexOptions.Compiled);

    public static bool IsValid(string id) => !string.IsNullOrWhiteSpace(id) && Pattern.IsMatch(id);

    public static Result<T> FailIfInvalid<T>(string id, string fieldName)
    {
        if (!IsValid(id))
            return Result.Failure<T>(Error.Validation(fieldName, $"{fieldName} must be a valid 24-character hex ObjectId."));
        return Result<T>.Success(default!); // sentinel — caller checks IsFailure
    }
}
