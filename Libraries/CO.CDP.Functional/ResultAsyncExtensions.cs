namespace CO.CDP.Functional;

public static class ResultAsyncExtensions
{
    /// <summary>Maps the success value of a <c>Task&lt;Result&gt;</c> pipeline.</summary>
    public static async Task<Result<TError, TNew>> MapResultAsync<TError, TValue, TNew>(
        this Task<Result<TError, TValue>> resultTask,
        Func<TValue, TNew> map) =>
        (await resultTask).Match(
            onLeft: error => Result<TError, TNew>.Failure(error),
            onRight: value => Result<TError, TNew>.Success(map(value)));

    /// <summary>Chains a Result-returning async step, short-circuiting on failure.</summary>
    public static async Task<Result<TError, TNew>> BindResultAsync<TError, TValue, TNew>(
        this Task<Result<TError, TValue>> resultTask,
        Func<TValue, Task<Result<TError, TNew>>> bind) =>
        await (await resultTask).MatchAsync(
            onLeft: error => Task.FromResult(Result<TError, TNew>.Failure(error)),
            onRight: bind);

    /// <summary>Filters a successful result in an async pipeline — becomes Failure if predicate fails.</summary>
    public static async Task<Result<TError, TValue>> EnsureAsync<TError, TValue>(
        this Task<Result<TError, TValue>> resultTask,
        Func<TValue, bool> predicate,
        Func<TError> onFail) =>
        (await resultTask).Ensure(predicate, onFail);

    /// <summary>Executes an async side-effect on the success value in an async pipeline, returning the result.</summary>
    public static async Task<Result<TError, TValue>> TapResultAsync<TError, TValue>(
        this Task<Result<TError, TValue>> resultTask,
        Func<TValue, Task> action) =>
        await (await resultTask).TapAsync(action);

    /// <summary>Extracts the value from a <c>Task&lt;Result&lt;Exception, TValue&gt;&gt;</c>, throwing on failure.</summary>
    public static async Task<TValue> UnwrapAsync<TValue>(
        this Task<Result<Exception, TValue>> resultTask) =>
        (await resultTask).Unwrap();
}