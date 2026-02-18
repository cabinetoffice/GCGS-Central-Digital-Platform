namespace CO.CDP.UserManagement.Core.Interfaces;

public interface IOrganisationPersonsSyncService
{
    /// <summary>
    /// Syncs UserOrganisationMembership records for all persons associated with the specified organisation.
    /// </summary>
    /// <param name="organisationCdpGuid">The CDP organisation GUID</param>
    /// <param name="organisationId">The organisation ID in UserManagement</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SyncOrganisationMembershipsAsync(
        Guid organisationCdpGuid,
        int organisationId,
        CancellationToken cancellationToken = default);
}
