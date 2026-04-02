using FluentValidation;
using MediatR;
using PrimeDiscipline.Application.Common;

namespace PrimeDiscipline.Application.Behaviors;

/// <summary>
/// MediatR pipeline behaviour that runs all registered FluentValidation validators
/// before the handler is invoked.  Aggregates all validation failures into a
/// <see cref="Result{T}"/> with a list of <see cref="Error"/> objects.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        ValidationContext<TRequest> context = new(request);

        IEnumerable<FluentValidation.Results.ValidationFailure> failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f is not null);

        List<FluentValidation.Results.ValidationFailure> failureList = [.. failures];

        if (failureList.Count == 0)
            return await next(cancellationToken);

        List<Error> errors = failureList
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToList();

        // The convention is that every handler returns Result<T>.
        // We create it via reflection to avoid a dependency on T at compile time.
        Type responseType   = typeof(TResponse);
        Type? genericArg    = responseType.IsGenericType ? responseType.GetGenericArguments()[0] : null;

        if (genericArg is not null)
        {
            System.Reflection.MethodInfo? failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), [typeof(IReadOnlyList<Error>)])
                ?.MakeGenericMethod(genericArg);

            if (failureMethod is not null)
            {
                TResponse? result = (TResponse?)failureMethod.Invoke(null, [errors]);
                return result!;
            }
        }

        throw new ValidationException(failureList);
    }
}
