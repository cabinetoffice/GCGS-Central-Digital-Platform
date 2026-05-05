namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record ClaimsTree(
    string UserPrincipalId,
    IEnumerable<OrganisationClaims> Organisations);

public record OrganisationClaims(
    Guid OrganisationId,
    string OrganisationName,
    string OrganisationRole,
    IEnumerable<ApplicationClaims> Applications);

public record ApplicationClaims(
    Guid ApplicationId,
    string ApplicationName,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions);
