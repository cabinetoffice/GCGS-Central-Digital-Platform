namespace CO.CDP.UserManagement.Shared.Requests;

/// <summary>
/// Request model for creating a permission.
/// </summary>
public record CreatePermissionRequest
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
    public bool IsActive { get; init; } = true;
}
