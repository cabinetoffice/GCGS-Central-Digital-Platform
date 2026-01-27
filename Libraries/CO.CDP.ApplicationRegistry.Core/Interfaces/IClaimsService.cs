using CO.CDP.ApplicationRegistry.Core.Models;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

/// <summary>
/// Service for resolving user claims across all organisations and applications.
/// </summary>
public interface IClaimsService
{
    /// <summary>
    /// Gets the complete set of claims for a user including all their organisation memberships,
    /// application assignments, roles, and permissions.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's complete claims.</returns>
    Task<UserClaims> GetUserClaimsAsync(string userPrincipalId, CancellationToken cancellationToken = default);
}
