using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Shared.Responses;

/// <summary>
/// Response model for a pending organisation invite.
/// </summary>
public record PendingOrganisationInviteResponse
{
    /// <summary>
    /// Gets or sets the pending invite identifier.
    /// </summary>
    public required int PendingInviteId { get; init; }

    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets or sets the CDP person invite identifier.
    /// </summary>
    public required Guid CdpPersonInviteGuid { get; init; }

    /// <summary>
    /// Gets or sets the invited user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets or sets the invited user's first name.
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Gets or sets the invited user's last name.
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Gets or sets the organisation role for the invite.
    /// </summary>
    public required OrganisationRole OrganisationRole { get; init; }

    /// <summary>
    /// Gets or sets the invite status.
    /// </summary>
    public required UserStatus Status { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user who invited this person.
    /// </summary>
    public string? InvitedBy { get; init; }

    /// <summary>
    /// Gets or sets when the invite expires.
    /// </summary>
    public DateTimeOffset? ExpiresOn { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
