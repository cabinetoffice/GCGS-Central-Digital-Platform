using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Service interface for orchestrating organisation user invites.
/// </summary>
public interface IInviteOrchestrationService
{
    /// <summary>
    /// Invites a user to an organisation and creates a pending invite record.
    /// </summary>
    Task<PendingOrganisationInvite> InviteUserAsync(
        Guid cdpOrganisationId,
        InviteUserRequest request,
        string? inviterPrincipalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a pending invite and revokes the CDP invite.
    /// </summary>
    Task RemoveInviteAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resends a pending invite and refreshes the CDP invite GUID.
    /// </summary>
    Task ResendInviteAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the organisation role for a pending invite.
    /// </summary>
    Task ChangeInviteRoleAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default);
}
