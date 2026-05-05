namespace CO.CDP.UserManagement.Shared.Responses;

/// <summary>
/// Response model for an organisation join request.
/// </summary>
public record JoinRequestResponse
{
    /// <summary>Gets or sets the join request identifier (from the Organisation API).</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets or sets the CDP person identifier of the requester.</summary>
    public required Guid PersonId { get; init; }

    /// <summary>Gets or sets the requester's first name.</summary>
    public required string FirstName { get; init; }

    /// <summary>Gets or sets the requester's last name.</summary>
    public required string LastName { get; init; }

    /// <summary>Gets or sets the requester's email address.</summary>
    public required string Email { get; init; }
}