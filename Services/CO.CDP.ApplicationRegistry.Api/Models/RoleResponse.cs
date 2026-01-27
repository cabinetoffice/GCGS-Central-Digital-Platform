namespace CO.CDP.ApplicationRegistry.Api.Models;

/// <summary>
/// Response model for a role.
/// </summary>
public record RoleResponse
{
    /// <summary>
    /// Gets or sets the role identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public required int ApplicationId { get; init; }

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

    /// <summary>
    /// Gets or sets the collection of permissions assigned to this role.
    /// </summary>
    public IEnumerable<PermissionResponse>? Permissions { get; init; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets or sets the created by user identifier.
    /// </summary>
    public required string CreatedBy { get; init; }

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; init; }

    /// <summary>
    /// Gets or sets the last modified by user identifier.
    /// </summary>
    public string? ModifiedBy { get; init; }
}