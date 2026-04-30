namespace CO.CDP.OrganisationSync;

using CO.CDP.Functional;

/// <summary>
/// Pipeline extensions for chaining UM repository results (Result&lt;string, Unit&gt;)
/// into the OrganisationSync Result pipeline (Result&lt;SyncError, T&gt;).
/// </summary>
public static class SyncPipelineExtensions
{
    /// <summary>Chains a UM Result step from a UM-typed receiver, mapping string error to SyncError.</summary>
    public static async Task<Result<SyncError, TNew>> BindUmResult<TValue, TNew>(
        this Task<Result<string, TValue>> resultTask,
        Func<TValue, Task<Result<string, TNew>>> bind) =>
        await (await resultTask).MatchAsync(
            onLeft: error => Task.FromResult(Result<SyncError, TNew>.Failure(new SyncFailureError(error))),
            onRight: async value => MapStringResult(await bind(value)));

    /// <summary>Chains a UM Result step from a SyncError-typed receiver (mid-pipeline).</summary>
    public static async Task<Result<SyncError, TNew>> BindUmResult<TValue, TNew>(
        this Task<Result<SyncError, TValue>> resultTask,
        Func<TValue, Task<Result<string, TNew>>> bind) =>
        await (await resultTask).MatchAsync(
            onLeft: error => Task.FromResult(Result<SyncError, TNew>.Failure(error)),
            onRight: async value => MapStringResult(await bind(value)));

    /// <summary>Maps the success value of a UM Result pipeline to a SyncError-typed Result.</summary>
    public static async Task<Result<SyncError, TNew>> MapToSyncResult<TValue, TNew>(
        this Task<Result<string, TValue>> resultTask,
        Func<TValue, TNew> map) =>
        (await resultTask).Match(
            onLeft: error => Result<SyncError, TNew>.Failure(new SyncFailureError(error)),
            onRight: value => Result<SyncError, TNew>.Success(map(value)));

    /// <summary>Maps the success value of a SyncError-typed pipeline (mid-pipeline).</summary>
    public static async Task<Result<SyncError, TNew>> MapToSyncResult<TValue, TNew>(
        this Task<Result<SyncError, TValue>> resultTask,
        Func<TValue, TNew> map) =>
        (await resultTask).Match(
            onLeft: error => Result<SyncError, TNew>.Failure(error),
            onRight: value => Result<SyncError, TNew>.Success(map(value)));

    private static Result<SyncError, T> MapStringResult<T>(Result<string, T> result) =>
        result.Match(
            onLeft: error => Result<SyncError, T>.Failure(new SyncFailureError(error)),
            onRight: value => Result<SyncError, T>.Success(value));
}
