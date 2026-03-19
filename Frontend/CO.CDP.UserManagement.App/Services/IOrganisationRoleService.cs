using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Services;

public interface IOrganisationRoleService
{
    Task<IReadOnlyList<OrganisationRoleDefinitionResponse>> GetRolesAsync(CancellationToken ct = default);
    Task<OrganisationRoleDefinitionResponse?> GetRoleAsync(OrganisationRole role, CancellationToken ct = default);
}
