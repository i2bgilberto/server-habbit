namespace PrimeDiscipline.Application.Common;

public sealed class Result<T>
{
    private Result(T value)
    {
        IsSuccess = true;
        Value     = value;
        Errors    = [];
    }

    private Result(IReadOnlyList<Error> errors)
    {
        IsSuccess = false;
        Value     = default;
        Errors    = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public IReadOnlyList<Error> Errors { get; }

    public Error FirstError => Errors.Count > 0 ? Errors[0] : Error.None;

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new([error]);

    public static Result<T> Failure(IReadOnlyList<Error> errors) => new(errors);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<IReadOnlyList<Error>, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Errors);
}

public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(IReadOnlyList<Error> errors) => Result<T>.Failure(errors);
}
