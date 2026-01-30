using CO.CDP.ApplicationRegistry.App.Mapping;
using CO.CDP.ApplicationRegistry.App.Models;
using CO.CDP.ApplicationRegistry.WebApiClient;

namespace CO.CDP.ApplicationRegistry.App.Services;

public sealed class ApplicationService(
    ApplicationRegistryClient apiClient) : IApplicationService
{
    public async Task<HomeViewModel?> GetHomeViewModelAsync(string organisationSlug, CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var apps = (await apiClient.ApplicationsAllAsync(org.Id, ct)).ToList();
            var roles = await GetAllRolesForApplicationsAsync(apps, ct);
            return ViewModelMapper.ToHomeViewModel(org, apps, roles);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<ApplicationsViewModel?> GetApplicationsViewModelAsync(
        string organisationSlug,
        string? selectedCategory = null,
        string? selectedStatus = null,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var allApps = (await apiClient.ApplicationsAllAsync(ct)).ToList();
            var enabledApps = (await apiClient.ApplicationsAllAsync(org.Id, ct)).ToList();
            return ViewModelMapper.ToApplicationsViewModel(org, allApps, enabledApps, selectedCategory, selectedStatus, searchTerm);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<RoleResponse>> GetAllRolesForApplicationsAsync(
        IReadOnlyList<OrganisationApplicationResponse> apps,
        CancellationToken ct)
    {
        var allRoles = new List<RoleResponse>();

        foreach (var app in apps.Where(a => a.IsActive))
        {
            try
            {
                var roles = (await apiClient.RolesAllAsync(app.ApplicationId, ct)).ToList();
                allRoles.AddRange(roles);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                // Application not found, skip
            }
        }

        return allRoles;
    }
}