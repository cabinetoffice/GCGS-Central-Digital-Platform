using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request body for approving or rejecting an organisation join request.
/// </summary>
public record ReviewJoinRequestRequest
{
    /// <summary>Gets or sets the review decision.</summary>
    public required JoinRequestDecision Decision { get; init; }

    /// <summary>Gets or sets the CDP person GUID of the user who submitted the join request.</summary>
    public required Guid RequestingPersonId { get; init; }
}