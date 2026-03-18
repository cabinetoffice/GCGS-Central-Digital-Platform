namespace CO.CDP.UserManagement.Core.Interfaces;

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
        CancellationToken cancellationToken = default);
}
