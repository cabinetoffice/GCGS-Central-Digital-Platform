using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Syncs UserManagement memberships to the CDP OrganisationPerson join table.
/// </summary>
public interface ICdpMembershipSyncService
{
    Task SyncMembershipCreatedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default);

    Task SyncMembershipRoleChangedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default);

    Task SyncMembershipRemovedAsync(UserOrganisationMembership membership, CancellationToken cancellationToken = default);
}
