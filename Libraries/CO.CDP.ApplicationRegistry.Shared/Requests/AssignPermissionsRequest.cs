namespace CO.CDP.ApplicationRegistry.Shared.Requests;

/// <summary>
/// Request model for assigning permissions to a role.
/// </summary>
public record AssignPermissionsRequest
{
    /// <summary>
    /// Gets or sets the collection of permission identifiers to assign.
    /// </summary>
    public required IEnumerable<int> PermissionIds { get; init; }
}
