namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request model for accepting a pending organisation invite.
/// </summary>
public record AcceptOrganisationInviteRequest
{
    /// <summary>
    /// Gets or sets the user principal identifier (subject from identity provider).
    /// </summary>
    public required string UserPrincipalId { get; init; }

    /// <summary>
    /// Gets or sets the CDP person identifier.
    /// </summary>
    public required Guid CdpPersonId { get; init; }
}
