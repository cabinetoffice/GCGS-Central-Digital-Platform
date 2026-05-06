namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record ProfileDto(
    string UserPrincipalId,
    IEnumerable<ProfileOrganisation> Organisations);

public record ProfileOrganisation(
    Guid OrganisationId,
    string OrganisationName,
    string Role);

public record ProfilePermissions(
    Guid OrganisationId,
    IEnumerable<ApplicationClaims> Applications);
