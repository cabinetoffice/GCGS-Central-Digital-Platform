using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Repository interface for UserApplicationAssignment entities.
/// </summary>
public interface IUserApplicationAssignmentRepository : IRepository<UserApplicationAssignment>
{
    /// <summary>
    /// Gets all assignments for a specific user in an organisation.
    /// </summary>
    /// <param name="userOrganisationMembershipId">The user organisation membership identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of user application assignments.</returns>
    Task<IEnumerable<UserApplicationAssignment>> GetByMembershipIdAsync(int userOrganisationMembershipId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assignments for multiple organisation memberships.
    /// </summary>
    /// <param name="userOrganisationMembershipIds">The user organisation membership identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of user application assignments.</returns>
    Task<IEnumerable<UserApplicationAssignment>> GetByMembershipIdsAsync(IEnumerable<int> userOrganisationMembershipIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user's assignment to an application in an organisation including roles.
    /// </summary>
    /// <param name="userOrganisationMembershipId">The user organisation membership identifier.</param>
    /// <param name="organisationApplicationId">The organisation application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The assignment if found; otherwise, null.</returns>
    Task<UserApplicationAssignment?> GetByMembershipAndApplicationAsync(int userOrganisationMembershipId, int organisationApplicationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all assignments for a user with full claim information.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of assignments with full details for claims generation.</returns>
    Task<IEnumerable<UserApplicationAssignment>> GetAssignmentsForClaimsAsync(string userPrincipalId, CancellationToken cancellationToken = default);
}
