namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request model for updating a permission.
/// </summary>
public record UpdatePermissionRequest
{
    /// <summary>
    /// Gets or sets the permission name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the permission description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets whether the permission is active.
    /// </summary>
    public required bool IsActive { get; init; }
}
