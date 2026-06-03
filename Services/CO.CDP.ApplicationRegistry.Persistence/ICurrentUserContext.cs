namespace CO.CDP.ApplicationRegistry.Persistence;

/// <summary>
/// Provides the identity of the currently authenticated caller.
/// Abstracts the persistence layer from ASP.NET Core's IHttpContextAccessor.
/// Registered as scoped — returns the authenticated caller's URN for write operations.
/// Falls back to "system" when no authenticated context exists (background workers, tests).
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>The authenticated caller's URN (sub claim), or "system" if unavailable.</summary>
    string UserId { get; }
}
