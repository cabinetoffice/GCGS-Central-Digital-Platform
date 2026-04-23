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
}
