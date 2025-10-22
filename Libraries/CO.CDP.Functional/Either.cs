namespace CO.CDP.Functional;

public abstract class Either<TLeft, TRight>
{
    public abstract TResult Match<TResult>(Func<TLeft, TResult> onLeft, Func<TRight, TResult> onRight);
    public abstract Task<TResult> MatchAsync<TResult>(Func<TLeft, Task<TResult>> onLeft, Func<TRight, Task<TResult>> onRight);
    public abstract void Match(Action<TLeft> onLeft, Action<TRight> onRight);

    public static Either<TLeft, TRight> Left(TLeft value) => new LeftCase(value);
    public static Either<TLeft, TRight> Right(TRight value) => new RightCase(value);

    private sealed class LeftCase(TLeft value) : Either<TLeft, TRight>
    {
        public override TResult Match<TResult>(Func<TLeft, TResult> onLeft, Func<TRight, TResult> onRight) => onLeft(value);
        public override async Task<TResult> MatchAsync<TResult>(Func<TLeft, Task<TResult>> onLeft, Func<TRight, Task<TResult>> onRight) => await onLeft(value);
        public override void Match(Action<TLeft> onLeft, Action<TRight> onRight) => onLeft(value);
    }

    private sealed class RightCase(TRight value) : Either<TLeft, TRight>
    {
        public override TResult Match<TResult>(Func<TLeft, TResult> onLeft, Func<TRight, TResult> onRight) => onRight(value);
        public override async Task<TResult> MatchAsync<TResult>(Func<TLeft, Task<TResult>> onLeft, Func<TRight, Task<TResult>> onRight) => await onRight(value);
        public override void Match(Action<TLeft> onLeft, Action<TRight> onRight) => onRight(value);
    }
}

public static class EitherExtensions
{
    public static Either<TLeft, TRightNew> Map<TLeft, TRight, TRightNew>(
        this Either<TLeft, TRight> either,
        Func<TRight, TRightNew> map)
    {
        return either.Match(
            left => Either<TLeft, TRightNew>.Left(left),
            right => Either<TLeft, TRightNew>.Right(map(right))
        );
    }

    public static async Task<Either<TLeft, TRightNew>> MapAsync<TLeft, TRight, TRightNew>(
        this Either<TLeft, TRight> either,
        Func<TRight, Task<TRightNew>> map)
    {
        return await either.MatchAsync(
            left => Task.FromResult(Either<TLeft, TRightNew>.Left(left)),
            async right => Either<TLeft, TRightNew>.Right(await map(right))
        );
    }

    public static Either<TLeft, TRightNew> Bind<TLeft, TRight, TRightNew>(
        this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, TRightNew>> bind)
    {
        return either.Match(
            left => Either<TLeft, TRightNew>.Left(left),
            bind
        );
    }

    public static async Task<Either<TLeft, TRightNew>> BindAsync<TLeft, TRight, TRightNew>(
        this Either<TLeft, TRight> either,
        Func<TRight, Task<Either<TLeft, TRightNew>>> bind)
    {
        return await either.MatchAsync(
            left => Task.FromResult(Either<TLeft, TRightNew>.Left(left)),
            bind
        );
    }

    public static Either<TLeft, TRight> OnLeft<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        Action<TLeft> action)
    {
        either.Match(action, _ => { });
        return either;
    }

    public static Either<TLeft, TRight> OnRight<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        Action<TRight> action)
    {
        either.Match(_ => { }, action);
        return either;
    }

    public static TRight GetOrElse<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        TRight defaultValue)
    {
        return either.Match(
            _ => defaultValue,
            right => right
        );
    }

    public static TRight GetOrElse<TLeft, TRight>(
        this Either<TLeft, TRight> either,
        Func<TLeft, TRight> defaultProvider)
    {
        return either.Match(
            defaultProvider,
            right => right
        );
    }

    public static bool IsLeft<TLeft, TRight>(this Either<TLeft, TRight> either)
    {
        return either.Match(_ => true, _ => false);
    }

    public static bool IsRight<TLeft, TRight>(this Either<TLeft, TRight> either)
    {
        return either.Match(_ => false, _ => true);
    }
}
