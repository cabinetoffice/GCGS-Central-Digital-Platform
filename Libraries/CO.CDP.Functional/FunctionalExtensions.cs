namespace CO.CDP.Functional;

public static class FunctionalExtensions
{
    public static async Task<TO> AndThen<TI, TO>(this Task<TI> on, Func<TI, TO> continuation) =>
        continuation(await on);
}