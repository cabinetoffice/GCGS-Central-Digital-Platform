using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Shared.Responses;

public record OrganisationRoleDefinitionResponse
{
    public required OrganisationRole Id { get; init; }
    public required string DisplayName { get; init; }
    public string? Description { get; init; }
}
