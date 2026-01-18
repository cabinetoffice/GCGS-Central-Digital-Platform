using CO.CDP.ApplicationRegistry.Core.Entities;

namespace CO.CDP.ApplicationRegistry.Api.Models;

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

/// <summary>
/// Extension methods for role mapping.
/// </summary>
public static class RoleMappingExtensions
{
    /// <summary>
    /// Converts an ApplicationRole entity to a RoleResponse.
    /// </summary>
    /// <param name="role">The application role entity.</param>
    /// <param name="includePermissions">Whether to include permissions in the response.</param>
    /// <returns>The role response model.</returns>
    public static RoleResponse ToResponse(this ApplicationRole role, bool includePermissions = true)
    {
        return new RoleResponse
        {
            Id = role.Id,
            ApplicationId = role.ApplicationId,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            Permissions = includePermissions && role.Permissions != null
                ? role.Permissions.Select(p => p.ToResponse())
                : null,
            CreatedAt = role.CreatedAt,
            CreatedBy = role.CreatedBy,
            ModifiedAt = role.ModifiedAt,
            ModifiedBy = role.ModifiedBy
        };
    }

    /// <summary>
    /// Converts a collection of ApplicationRole entities to RoleResponse models.
    /// </summary>
    /// <param name="roles">The collection of application role entities.</param>
    /// <param name="includePermissions">Whether to include permissions in the responses.</param>
    /// <returns>The collection of role response models.</returns>
    public static IEnumerable<RoleResponse> ToResponses(this IEnumerable<ApplicationRole> roles, bool includePermissions = true)
    {
        return roles.Select(r => r.ToResponse(includePermissions));
    }
}
