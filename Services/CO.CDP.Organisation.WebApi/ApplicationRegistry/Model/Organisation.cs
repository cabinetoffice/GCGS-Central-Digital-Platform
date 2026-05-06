namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record OrganisationDto(
    Guid Id,
    string Name,
    string Slug,
    string? Type,
    Guid? ParentOrganisationId,
    bool IsActive,
    DateTimeOffset CreatedOn,
    DateTimeOffset UpdatedOn);

public record CreateOrganisation(
    string Name,
    string? Type,
    Guid? ParentOrganisationId);

public record UpdateOrganisation(
    string? Name,
    string? Type,
    Guid? ParentOrganisationId,
    bool? IsActive);

public record MemberDto(
    Guid Id,
    string UserPrincipalId,
    string OrganisationRole,
    DateTimeOffset JoinedAt,
    bool IsActive);

public record AddMember(
    string UserPrincipalId,
    string OrganisationRole);

public record UpdateMember(
    string OrganisationRole);
