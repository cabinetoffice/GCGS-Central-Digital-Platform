namespace CO.CDP.ApplicationRegistry.Shared.Responses;

/// <summary>
/// Response model for a permission.
/// </summary>
public record PermissionResponse
{
    /// <summary>
    /// Gets or sets the permission identifier.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    public required int ApplicationId { get; init; }

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
