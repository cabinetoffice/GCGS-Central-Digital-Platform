using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Shared.Responses;

namespace CO.CDP.ApplicationRegistry.Api.Models;

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