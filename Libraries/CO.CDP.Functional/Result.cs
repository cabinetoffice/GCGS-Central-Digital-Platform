namespace CO.CDP.Functional;

public class Result<TError, TValue> : Either<TError, TValue>
{
    private readonly Either<TError, TValue> _either;

    protected Result(Either<TError, TValue> either)
    {
        _either = either;
    }

    public bool IsSuccess => _either.IsRight();
    public bool IsFailure => _either.IsLeft();

    public static Result<TError, TValue> Success(TValue value) => new(Right(value));
    public static Result<TError, TValue> Failure(TError error) => new(Left(error));

    public override TResult Match<TResult>(Func<TError, TResult> onLeft, Func<TValue, TResult> onRight)
        => _either.Match(onLeft, onRight);

    public override Task<TResult> MatchAsync<TResult>(Func<TError, Task<TResult>> onLeft,
        Func<TValue, Task<TResult>> onRight)
        => _either.MatchAsync(onLeft, onRight);

    public override void Match(Action<TError> onLeft, Action<TValue> onRight)
        => _either.Match(onLeft, onRight);
}

public static class ResultExtensions
{
    /// <summary>Lifts a nullable reference into a Result, using the error factory if null.</summary>
    public static Result<TError, TValue> ToResult<TError, TValue>(
        this TValue? value, Func<TError> onNull) where TValue : class =>
        value is null ? Result<TError, TValue>.Failure(onNull()) : Result<TError, TValue>.Success(value);

    /// <summary>Filters a successful result — if the predicate fails, becomes Failure.</summary>
    public static Result<TError, TValue> Ensure<TError, TValue>(
        this Result<TError, TValue> result, Func<TValue, bool> predicate, Func<TError> onFail) =>
        result.Match(
            onLeft: _ => result,
            onRight: value => predicate(value) ? result : Result<TError, TValue>.Failure(onFail()));

    /// <summary>Executes a synchronous side-effect on the success value, returning the original result.</summary>
    public static Result<TError, TValue> Tap<TError, TValue>(
        this Result<TError, TValue> result, Action<TValue> action) =>
        result.Match(
            onLeft: _ => result,
            onRight: value =>
            {
                action(value);
                return result;
            });

    /// <summary>Executes an async side-effect on the success value, returning the original result.</summary>
    public static async Task<Result<TError, TValue>> TapAsync<TError, TValue>(
        this Result<TError, TValue> result, Func<TValue, Task> action) =>
        await result.MatchAsync(
            onLeft: _ => Task.FromResult(result),
            onRight: async value =>
            {
                await action(value);
                return result;
            });

    /// <summary>Extracts the value from a <c>Result&lt;Exception, TValue&gt;</c>, throwing the exception on failure.</summary>
    public static TValue Unwrap<TValue>(this Result<Exception, TValue> result) =>
        result.Match(
            onLeft: ex => throw ex,
            onRight: value => value);

    public static TValue GetOrElse<TError, TValue>(
        this Result<TError, TValue> result,
        TValue defaultValue)
    {
        return result.Match(
            _ => defaultValue,
            value => value
        );
    }

    public static TValue GetOrElse<TError, TValue>(
        this Result<TError, TValue> result,
        Func<TError, TValue> defaultProvider)
    {
        return result.Match(
            defaultProvider,
            value => value
        );
    }
}