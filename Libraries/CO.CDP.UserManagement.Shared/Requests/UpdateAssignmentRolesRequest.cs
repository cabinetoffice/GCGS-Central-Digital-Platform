namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request model for updating a user's assignment roles.
/// </summary>
public record UpdateAssignmentRolesRequest
{
    /// <summary>
    /// Gets or sets the collection of role identifiers.
    /// </summary>
    public required IEnumerable<int> RoleIds { get; init; }
}
