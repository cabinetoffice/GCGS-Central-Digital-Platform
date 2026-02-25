using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request model for inviting a user to an organisation.
/// </summary>
public record InviteUserRequest
{
    /// <summary>
    /// Gets or sets the invited user's first name.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Gets or sets the invited user's last name.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Gets or sets the invited user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets or sets the organisation role for the invited user.
    /// </summary>
    public OrganisationRole OrganisationRole { get; init; }

    /// <summary>
    /// Gets or sets the optional application role assignments for the invited user.
    /// These will be applied when the invite is accepted.
    /// </summary>
    public List<ApplicationAssignment>? ApplicationAssignments { get; init; }
}

/// <summary>
/// Represents an application role assignment for an invite.
/// </summary>
public record ApplicationAssignment
{
    /// <summary>
    /// Gets or sets the organisation application identifier.
    /// </summary>
    public required int OrganisationApplicationId { get; init; }

    /// <summary>
    /// Gets or sets the application role identifiers to assign.
    /// </summary>
    public required List<int> ApplicationRoleIds { get; init; }
}
