namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request to update multiple application assignments for a user or invite.
/// </summary>
public record UpdateUserAssignmentsRequest
{
    public required IEnumerable<ApplicationRoleAssignment> Assignments { get; init; }
}

public record ApplicationRoleAssignment
{
    /// <summary>
    /// OrganisationApplicationId (the per-organisation application record id)
    /// </summary>
    public required int OrganisationApplicationId { get; init; }

    /// <summary>
    /// ApplicationId (the global application id) - required when creating a new assignment.
    /// </summary>
    public int? ApplicationId { get; init; }

    /// <summary>
    /// Role identifiers to apply for this assignment.
    /// </summary>
    public required IEnumerable<int> RoleIds { get; init; }
}