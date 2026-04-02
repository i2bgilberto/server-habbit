using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrimeDiscipline.Application.Common;

namespace PrimeDiscipline.WebAPI.Extensions;

/// <summary>
/// Maps a <see cref="Result{T}"/> to the appropriate HTTP response.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, int successStatus = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return successStatus switch
            {
                StatusCodes.Status201Created => Results.Created(string.Empty, result.Value),
                StatusCodes.Status204NoContent => Results.NoContent(),
                _ => Results.Ok(result.Value),
            };
        }

        return result.FirstError.Code switch
        {
            var c when c.StartsWith("VALIDATION")  => Results.UnprocessableEntity(BuildProblem(result.Errors)),
            "NOT_FOUND"                             => Results.NotFound(BuildProblem(result.Errors)),
            "CONFLICT"                              => Results.Conflict(BuildProblem(result.Errors)),
            "FORBIDDEN"                             => Results.Json(BuildProblem(result.Errors), statusCode: StatusCodes.Status403Forbidden),
            _                                       => Results.BadRequest(BuildProblem(result.Errors)),
        };
    }

    private static ProblemDetails BuildProblem(IReadOnlyList<Error> errors) => new()
    {
        Title  = errors[0].Code,
        Detail = string.Join("; ", errors.Select(e => e.Message)),
        Extensions = { ["errors"] = errors.Select(e => new { e.Code, e.Message }).ToList() },
    };
}
