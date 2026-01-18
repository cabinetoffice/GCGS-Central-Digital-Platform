namespace CO.CDP.ApplicationRegistry.Core.Constants;

/// <summary>
/// Constants for cache keys used throughout the application.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Gets the cache key for a user's claims.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <returns>The cache key for the user's claims.</returns>
    public static string UserClaims(string userPrincipalId)
        => $"claims:user:{userPrincipalId}";

    /// <summary>
    /// Gets the cache key for an organisation's users.
    /// </summary>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <returns>The cache key for the organisation's users.</returns>
    public static string OrganisationUsers(Guid organisationId)
        => $"claims:org:{organisationId}";

    /// <summary>
    /// Gets the cache key for an application's users.
    /// </summary>
    /// <param name="applicationId">The application identifier.</param>
    /// <returns>The cache key for the application's users.</returns>
    public static string ApplicationUsers(Guid applicationId)
        => $"claims:app:{applicationId}";
}
