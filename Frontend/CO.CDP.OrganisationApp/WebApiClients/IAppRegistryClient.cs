namespace CO.CDP.OrganisationApp.WebApiClients;

// ── DTOs (mirrors ApplicationRegistry API model records) ──────────────────

public record AppRegistryApplicationDto(
    Guid Id,
    string Name,
    string ClientId,
    string? Description,
    string? Category,
    bool IsActive,
    DateTimeOffset CreatedOn,
    DateTimeOffset UpdatedOn);

public record AppRegistryRoleDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string? Description,
    bool IsActive,
    IEnumerable<AppRegistryPermissionDto> Permissions);

public record AppRegistryPermissionDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string? Description);

public record AppRegistryUserAssignmentDto(
    Guid Id,
    string UserPrincipalId,
    Guid ApplicationId,
    Guid OrganisationId,
    DateTimeOffset AssignedAt,
    string AssignedBy,
    bool IsActive,
    IEnumerable<AppRegistryRoleDto> Roles);

public record AppRegistryMemberDto(
    Guid Id,
    string UserPrincipalId,
    string OrganisationRole,
    DateTimeOffset JoinedAt,
    bool IsActive);

public record CreateAppRegistryUserAssignment(
    string UserPrincipalId,
    IEnumerable<Guid>? RoleIds);

public record UpdateAppRegistryUserAssignment(
    IEnumerable<Guid> RoleIds);

// ── Interface ─────────────────────────────────────────────────────────────

/// <summary>
/// HTTP client for the ApplicationRegistry API endpoints
/// (all /api/... routes on CO.CDP.Organisation.WebApi, backed by MongoDB).
/// </summary>
public interface IAppRegistryClient
{
    Task<ICollection<AppRegistryApplicationDto>> GetOrganisationApplicationsAsync(Guid orgId);
    Task<AppRegistryApplicationDto?> GetApplicationAsync(Guid appId);
    Task<ICollection<AppRegistryRoleDto>> GetApplicationRolesAsync(Guid appId);
    Task<ICollection<AppRegistryUserAssignmentDto>> GetUserAssignmentsAsync(Guid orgId, Guid appId);
    Task<ICollection<AppRegistryMemberDto>> GetOrganisationMembersAsync(Guid orgId);
    Task<AppRegistryUserAssignmentDto> AssignUserAsync(Guid orgId, Guid appId, CreateAppRegistryUserAssignment cmd);
    Task UpdateUserRolesAsync(Guid orgId, Guid appId, string userId, UpdateAppRegistryUserAssignment cmd);
    Task RevokeUserAsync(Guid orgId, Guid appId, string userId);
}
