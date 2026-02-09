using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Service for managing user assignments to applications with roles.
/// </summary>
public interface IUserAssignmentService
{
    /// <summary>
    /// Gets all assignments for a user within an organisation.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of user application assignments.</returns>
    Task<IEnumerable<UserApplicationAssignment>> GetUserAssignmentsAsync(string userPrincipalId, int organisationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a user to an application within an organisation with specific roles.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier.</param>
    /// <param name="organisationId">The organisation identifier.</param>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="roleIds">The collection of role identifiers to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user application assignment.</returns>
    Task<UserApplicationAssignment> AssignUserAsync(string userPrincipalId, int organisationId, int applicationId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's role assignments for an application.
    /// </summary>
    /// <param name="assignmentId">The assignment identifier.</param>
    /// <param name="roleIds">The new collection of role identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated assignment.</returns>
    Task<UserApplicationAssignment> UpdateAssignmentAsync(int assignmentId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a user's assignment to an application.
    /// </summary>
    /// <param name="assignmentId">The assignment identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RevokeAssignmentAsync(int assignmentId, CancellationToken cancellationToken = default);
}
