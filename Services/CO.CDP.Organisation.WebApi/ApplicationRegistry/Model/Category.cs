namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string TaxonomyType,
    bool IsActive);

public record CreateCategory(
    string Name,
    string? Description,
    string TaxonomyType);

public record UpdateCategory(
    string? Name,
    string? Description,
    string? TaxonomyType,
    bool? IsActive);

public record CategoryPermissionDto(
    Guid Id,
    Guid OrganisationTypeId,
    string PermissionLevel,
    Guid GrantedBy,
    DateTimeOffset GrantedAt);

public record SetCategoryPermissions(
    IEnumerable<CategoryPermissionInput> Permissions);

public record CategoryPermissionInput(
    Guid OrganisationTypeId,
    string PermissionLevel);
