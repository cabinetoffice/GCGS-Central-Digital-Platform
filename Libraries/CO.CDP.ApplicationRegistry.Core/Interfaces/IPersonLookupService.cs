using CO.CDP.ApplicationRegistry.Core.Models;

namespace CO.CDP.ApplicationRegistry.Core.Interfaces;

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
}
