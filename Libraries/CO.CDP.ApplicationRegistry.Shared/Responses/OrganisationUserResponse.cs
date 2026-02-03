using CO.CDP.ApplicationRegistry.Shared.Enums;

namespace CO.CDP.ApplicationRegistry.Shared.Responses;

/// <summary>
/// Response model for an organisation user membership.
/// </summary>
public record OrganisationUserResponse
{
    /// <summary>
    /// Gets or sets the user organisation membership identifier.
    /// </summary>
    public required int MembershipId { get; init; }

    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public required int OrganisationId { get; init; }

    /// <summary>
    /// Gets or sets the user principal identifier.
    /// </summary>
    public required string UserPrincipalId { get; init; }

    /// <summary>
    /// Gets or sets the organisation role for the user.
    /// </summary>
    public required OrganisationRole OrganisationRole { get; init; }

    /// <summary>
    /// Gets or sets the user status within the organisation.
    /// </summary>
    public required UserStatus Status { get; init; }

    /// <summary>
    /// Gets or sets whether the membership is active.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets or sets the date and time when the user joined the organisation.
    /// </summary>
    public DateTimeOffset? JoinedAt { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user who invited this member.
    /// </summary>
    public string? InvitedBy { get; init; }

    /// <summary>
    /// Gets or sets the collection of application assignments for the user.
    /// </summary>
    public IEnumerable<UserAssignmentResponse>? ApplicationAssignments { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
