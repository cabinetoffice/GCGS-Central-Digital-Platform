namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request model for creating a role.
/// </summary>
public record CreateRoleRequest
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
    public bool IsActive { get; init; } = true;
}
