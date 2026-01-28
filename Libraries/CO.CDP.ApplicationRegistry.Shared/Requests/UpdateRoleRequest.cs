namespace CO.CDP.ApplicationRegistry.Shared.Requests;

/// <summary>
/// Request model for updating a role.
/// </summary>
public record UpdateRoleRequest
{
    /// <summary>
    /// Gets or sets the role name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the role description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets whether the role is active.
    /// </summary>
    public required bool IsActive { get; init; }
}
