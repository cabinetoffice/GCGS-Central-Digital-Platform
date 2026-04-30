namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Writes Organisation Information <c>OrganisationPerson</c> records on behalf of User Management,
/// preserving any externally-managed scopes (e.g. SUPERADMIN, SUPPORTADMIN) that UM does not own.
/// </summary>
public interface IOrganisationPersonSyncRepository
{
    /// <summary>
    /// Creates or updates the OrganisationPerson row, replacing only UM-managed scopes with
    /// <paramref name="computedScopes"/> while keeping any external scopes intact.
    /// </summary>
    Task UpsertAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        IReadOnlyList<string> computedScopes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all UM-managed scopes from the OrganisationPerson row.
    /// Deletes the row entirely only when no external scopes remain.
    /// </summary>
    Task RemoveAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        CancellationToken cancellationToken = default);
}
