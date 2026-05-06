namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record ApplicationDto(
    Guid Id,
    string Name,
    string ClientId,
    string? Description,
    string? Category,
    bool IsActive,
    DateTimeOffset CreatedOn,
    DateTimeOffset UpdatedOn);

public record CreateApplication(
    string Name,
    string ClientId,
    string? Description,
    string? Category);

public record UpdateApplication(
    string? Name,
    string? Description,
    string? Category,
    bool? IsActive);

public record PermissionDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string? Description);

public record CreatePermission(
    string Name,
    string? Description);

public record UpdatePermission(
    string? Name,
    string? Description);

public record RoleDto(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string? Description,
    bool IsActive,
    IEnumerable<PermissionDto> Permissions);

public record CreateRole(
    string Name,
    string? Description);

public record UpdateRole(
    string? Name,
    string? Description,
    bool? IsActive);

public record SetRolePermissions(
    IEnumerable<Guid> PermissionIds);
