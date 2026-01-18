using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Api.Models;

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

/// <summary>
/// Extension methods for permission mapping.
/// </summary>
public static class PermissionMappingExtensions
{
    /// <summary>
    /// Converts an ApplicationPermission entity to a PermissionResponse.
    /// </summary>
    /// <param name="permission">The application permission entity.</param>
    /// <returns>The permission response model.</returns>
    public static PermissionResponse ToResponse(this ApplicationPermission permission)
    {
        return new PermissionResponse
        {
            Id = permission.Id,
            ApplicationId = permission.ApplicationId,
            Name = permission.Name,
            Description = permission.Description,
            IsActive = permission.IsActive,
            CreatedAt = permission.CreatedAt,
            CreatedBy = permission.CreatedBy,
            ModifiedAt = permission.ModifiedAt,
            ModifiedBy = permission.ModifiedBy
        };
    }

    /// <summary>
    /// Converts a collection of ApplicationPermission entities to PermissionResponse models.
    /// </summary>
    /// <param name="permissions">The collection of application permission entities.</param>
    /// <returns>The collection of permission response models.</returns>
    public static IEnumerable<PermissionResponse> ToResponses(this IEnumerable<ApplicationPermission> permissions)
    {
        return permissions.Select(p => p.ToResponse());
    }
}
