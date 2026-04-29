namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Orchestrates the approval and rejection of organisation join requests.
/// </summary>
public interface IJoinRequestOrchestrationService
{
    /// <summary>
    /// Approves a join request. Calls the Organisation API to update status and trigger Notify emails,
    /// then creates a <c>UserOrganisationMembership</c> with the default Member role.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation GUID.</param>
    /// <param name="joinRequestId">The OI join request GUID.</param>
    /// <param name="requestingPersonId">The OI person GUID of the user who made the join request.</param>
    /// <param name="reviewerPrincipalId">The UserPrincipalId (URN) of the admin reviewing the request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ApproveJoinRequestAsync(
        Guid cdpOrganisationId,
        Guid joinRequestId,
        Guid requestingPersonId,
        string reviewerPrincipalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a join request. Calls the Organisation API to update status and trigger the Notify email.
    /// No UM state is changed on rejection.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation GUID.</param>
    /// <param name="joinRequestId">The OI join request GUID.</param>
    /// <param name="requestingPersonId">The OI person GUID of the user who made the join request.</param>
    /// <param name="reviewerPrincipalId">The UserPrincipalId (URN) of the admin reviewing the request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RejectJoinRequestAsync(
        Guid cdpOrganisationId,
        Guid joinRequestId,
        Guid requestingPersonId,
        string reviewerPrincipalId,
        CancellationToken cancellationToken = default);
}