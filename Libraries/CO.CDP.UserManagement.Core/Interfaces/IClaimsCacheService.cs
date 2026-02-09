using CO.CDP.UserManagement.Core.Models;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Caching decorator for claims service with TTL support.
/// </summary>
public interface IClaimsCacheService
{
    /// <summary>
    /// Gets the complete set of claims for a user from cache if available, otherwise from the service.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's complete claims.</returns>
    Task<UserClaims> GetUserClaimsAsync(string userPrincipalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the cache for a specific user.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InvalidateCacheAsync(string userPrincipalId, CancellationToken cancellationToken = default);
}
