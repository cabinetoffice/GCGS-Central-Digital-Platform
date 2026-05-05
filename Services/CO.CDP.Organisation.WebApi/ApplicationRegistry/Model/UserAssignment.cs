namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record UserAssignmentDto(
    Guid Id,
    string UserPrincipalId,
    Guid ApplicationId,
    Guid OrganisationId,
    DateTimeOffset AssignedAt,
    string AssignedBy,
    bool IsActive,
    IEnumerable<RoleDto> Roles);

public record CreateUserAssignment(
    string UserPrincipalId,
    IEnumerable<Guid>? RoleIds);

public record UpdateUserAssignment(
    IEnumerable<Guid> RoleIds);
