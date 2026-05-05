namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record AccessControlEntryDto(
    Guid Id,
    Guid ReportId,
    Guid UserPrincipal,
    Guid OrganisationId,
    Guid GrantedBy,
    DateTimeOffset GrantedAt,
    DateTimeOffset? RevokedAt);

public record GrantAccess(
    Guid UserPrincipal,
    Guid OrganisationId);
