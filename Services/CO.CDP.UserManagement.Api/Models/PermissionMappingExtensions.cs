using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.ApplicationRegistry.Shared.Responses;

namespace CO.CDP.UserManagement.Api.Models;

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