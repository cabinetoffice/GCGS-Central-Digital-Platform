namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Service for accessing current user information from the HTTP context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's principal identifier.
    /// </summary>
    /// <returns>The user principal identifier, or null if not authenticated.</returns>
    string? GetUserPrincipalId();

    /// <summary>
    /// Gets the current user's email address.
    /// </summary>
    /// <returns>The user email address, or null if not available.</returns>
    string? GetUserEmail();

    /// <summary>
    /// Determines whether the current user is authenticated.
    /// </summary>
    /// <returns>True if the user is authenticated; otherwise, false.</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Gets the current user's identifier for audit purposes.
    /// </summary>
    /// <returns>The user identifier, or null if not authenticated.</returns>
    string? GetCurrentUserId();
}
