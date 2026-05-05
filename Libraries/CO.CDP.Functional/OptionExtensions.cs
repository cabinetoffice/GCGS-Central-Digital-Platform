namespace CO.CDP.Functional;

/// <summary>Static factory helpers for <see cref="Option{T}"/>.</summary>
public static class Option
{
    /// <summary>Lifts a nullable reference type into an <see cref="Option{T}"/>.</summary>
    public static Option<T> From<T>(T? value) where T : class =>
        value is null ? Option<T>.None : Option<T>.Some(value);

    /// <summary>Lifts a nullable value type into an <see cref="Option{T}"/>.</summary>
    public static Option<T> From<T>(T? value) where T : struct =>
        value.HasValue ? Option<T>.Some(value.Value) : Option<T>.None;
}

public static class OptionExtensions
{
    /// <summary>Transforms the inner value if present; otherwise propagates None.</summary>
    public static Option<TOut> Map<TIn, TOut>(this Option<TIn> option, Func<TIn, TOut> map) =>
        option.Match(
            some: value => Option<TOut>.Some(map(value)),
            none: () => Option<TOut>.None);

    /// <summary>Chains an Option-returning function, short-circuiting on None.</summary>
    public static Option<TOut> Bind<TIn, TOut>(this Option<TIn> option, Func<TIn, Option<TOut>> bind) =>
        option.Match(
            some: bind,
            none: () => Option<TOut>.None);

    /// <summary>Chains an async Option-returning function, short-circuiting on None.</summary>
    public static Task<Option<TOut>> BindAsync<TIn, TOut>(
        this Option<TIn> option, Func<TIn, Task<Option<TOut>>> bind) =>
        option.MatchAsync(
            some: bind,
            none: () => Task.FromResult(Option<TOut>.None));

    /// <summary>Filters the option to None if the predicate is not satisfied.</summary>
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Match(
            some: value => predicate(value) ? Option<T>.Some(value) : Option<T>.None,
            none: () => Option<T>.None);

    /// <summary>Executes an async side-effect if the option has a value, then returns <see cref="Task"/>.</summary>
    public static Task TapAsync<T>(this Option<T> option, Func<T, Task> action) =>
        option.MatchAsync(
            some: action,
            none: () => Task.CompletedTask);

    /// <summary>Executes an async side-effect if the option has a value when awaiting the Option task.</summary>
    public static async Task TapAsync<T>(this Task<Option<T>> optionTask, Func<T, Task> action) =>
        await (await optionTask).TapAsync(action);

    /// <summary>Asynchronously transforms the inner value if present.</summary>
    public static async Task<Option<TOut>> MapAsync<TIn, TOut>(
        this Option<TIn> option, Func<TIn, Task<TOut>> map) =>
        await option.MatchAsync(
            some: async value => Option<TOut>.Some(await map(value)),
            none: () => Task.FromResult(Option<TOut>.None));
}