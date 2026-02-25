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
    /// Invites a user to an organisation via CDP bridge and creates an invite role mapping.
    /// </summary>
    Task<InviteRoleMapping> InviteUserAsync(
        Guid cdpOrganisationId,
        InviteUserRequest request,
        string? inviterPrincipalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an invite role mapping and revokes the CDP invite.
    /// </summary>
    Task RemoveInviteAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the organisation role for an invite role mapping.
    /// </summary>
    Task ChangeInviteRoleAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Accepts an invite and creates a user organisation membership.
    /// </summary>
    Task AcceptInviteAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        AcceptOrganisationInviteRequest request,
        CancellationToken cancellationToken = default);
}
