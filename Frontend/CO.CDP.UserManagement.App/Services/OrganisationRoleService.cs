using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using ApiClient = CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Services;

public class OrganisationRoleService(ApiClient.UserManagementClient apiClient) : IOrganisationRoleService
{
    public async Task<IReadOnlyList<OrganisationRoleDefinitionResponse>> GetRolesAsync(CancellationToken ct = default)
    {
        return (await apiClient.OrganisationRolesAsync(ct)).ToList();
    }

    public async Task<OrganisationRoleDefinitionResponse?> GetRoleAsync(OrganisationRole role, CancellationToken ct = default)
    {
        var roles = await GetRolesAsync(ct);
        return roles.FirstOrDefault(option => option.Id == role);
    }
}
