namespace CO.CDP.Common;

public static class FunctionalExtensions
{
    public static async Task<TO> AndThen<TI, TO>(this Task<TI> on, Func<TI, TO> continuation) =>
        continuation(await on);
}