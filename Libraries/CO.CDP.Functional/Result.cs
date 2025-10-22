namespace CO.CDP.Functional;

public class Result<TError, TValue> : Either<TError, TValue>
{
    private readonly Either<TError, TValue> _either;

    protected Result(Either<TError, TValue> either)
    {
        _either = either;
    }

    public static Result<TError, TValue> Success(TValue value) => new(Right(value));
    public static Result<TError, TValue> Failure(TError error) => new(Left(error));

    public override TResult Match<TResult>(Func<TError, TResult> onLeft, Func<TValue, TResult> onRight)
        => _either.Match(onLeft, onRight);

    public override Task<TResult> MatchAsync<TResult>(Func<TError, Task<TResult>> onLeft, Func<TValue, Task<TResult>> onRight)
        => _either.MatchAsync(onLeft, onRight);

    public override void Match(Action<TError> onLeft, Action<TValue> onRight)
        => _either.Match(onLeft, onRight);
}

public static class ResultExtensions
{
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
