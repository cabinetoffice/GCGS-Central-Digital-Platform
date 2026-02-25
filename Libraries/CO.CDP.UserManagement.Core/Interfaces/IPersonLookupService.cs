using CO.CDP.UserManagement.Core.Models;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Service for looking up person details from the CDP Person service.
/// </summary>
public interface IPersonLookupService
{
    /// <summary>
    /// Retrieves person details by their user principal identifier.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier (OneLogin 'sub' claim).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Person details if found, otherwise null.</returns>
    Task<PersonDetails?> GetPersonDetailsAsync(string userPrincipalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves person details by their CDP person identifiers.
    /// </summary>
    /// <param name="cdpPersonIds">The CDP person identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Person details keyed by CDP person identifier.</returns>
    Task<IReadOnlyDictionary<Guid, PersonDetails>> GetPersonDetailsByIdsAsync(
        IEnumerable<Guid> cdpPersonIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves person details by their email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Person details if found, otherwise null.</returns>
    Task<PersonDetails?> GetPersonDetailsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
