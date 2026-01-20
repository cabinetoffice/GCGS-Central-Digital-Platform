namespace CO.CDP.Functional;

public delegate bool TryParseDelegate<T>(string? value, out T result);

public static class FunctionalExtensions
{
    public static async Task<TO> AndThen<TI, TO>(this Task<TI> on, Func<TI, TO> continuation) =>
        continuation(await on);

    /// <summary>
    /// Attempts to parse a string using the provided TryParse delegate, returning an Option representing success or failure.
    /// </summary>
    /// <typeparam name="T">The type to parse to</typeparam>
    /// <param name="value">The string value to parse</param>
    /// <param name="tryParse">The TryParse delegate to use for parsing</param>
    /// <returns>An Option containing the parsed value if successful, otherwise None</returns>
    /// <example>
    /// var result = "123".TryParse(int.TryParse);
    /// var result = "3.14".TryParse(double.TryParse);
    /// </example>
    public static Option<T> TryParse<T>(this string? value, TryParseDelegate<T> tryParse) =>
        tryParse(value, out var result)
            ? Option<T>.Some(result)
            : Option<T>.None;
}