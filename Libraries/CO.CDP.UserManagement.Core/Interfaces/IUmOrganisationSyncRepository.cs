namespace CO.CDP.UserManagement.Core.Interfaces;

using PartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

/// <summary>
/// Keeps the User Management <c>organisations</c> table in sync when organisations are
/// created or renamed in Organisation Information.
/// </summary>
public interface IUmOrganisationSyncRepository
{
    /// <summary>
    /// Creates a UM organisation row for <paramref name="cdpGuid"/> if one does not already exist.
    /// </summary>
    Task EnsureCreatedAsync(Guid cdpGuid, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the synced organisation has all active, non-deleted applications enabled in User Management.
    /// </summary>
    Task EnsureActiveApplicationsEnabledAsync(Guid cdpGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the UM organisation name (and slug) when the name has changed.
    /// Falls back to <see cref="EnsureCreatedAsync"/> if the row does not yet exist.
    /// </summary>
    Task EnsureNameSyncedAsync(Guid cdpGuid, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the founding person's Owner membership for the synced organisation if one does not already exist.
    /// </summary>
    Task EnsureFounderOwnerCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyCollection<PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a member membership for a person who has claimed an invite, resolving the UM
    /// <c>OrganisationRole</c> from the invite's OI scopes via the <c>organisation_roles</c> table.
    /// </summary>
    Task EnsureMemberCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyList<string> inviteScopes,
        IReadOnlyCollection<PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken = default);
}
