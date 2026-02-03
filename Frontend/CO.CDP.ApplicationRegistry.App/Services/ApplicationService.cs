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

    public async Task<EnableApplicationViewModel?> GetEnableApplicationViewModelAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var allApps = (await apiClient.ApplicationsAllAsync(ct)).ToList();
            var app = allApps.FirstOrDefault(a => a.ClientId == applicationSlug);

            if (app == null) return null;

            var roles = (await apiClient.RolesAllAsync(app.Id, ct)).ToList();
            return ViewModelMapper.ToEnableApplicationViewModel(org, app, roles);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<bool> EnableApplicationAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var allApps = (await apiClient.ApplicationsAllAsync(ct)).ToList();
            var app = allApps.FirstOrDefault(a => a.ClientId == applicationSlug);

            if (app == null) return false;

            var request = new EnableApplicationRequest(app.Id);
            await apiClient.ApplicationsPOSTAsync(org.Id, request, ct);
            return true;
        }
        catch (ApiException)
        {
            return false;
        }
    }

    public async Task<EnableApplicationSuccessViewModel?> GetEnableSuccessViewModelAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var allApps = (await apiClient.ApplicationsAllAsync(ct)).ToList();
            var app = allApps.FirstOrDefault(a => a.ClientId == applicationSlug);

            if (app == null) return null;

            return ViewModelMapper.ToEnableSuccessViewModel(org, app);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<ApplicationDetailsViewModel?> GetApplicationDetailsViewModelAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var enabledApps = (await apiClient.ApplicationsAllAsync(org.Id, ct)).ToList();
            var orgApp = enabledApps.FirstOrDefault(oa => oa.Application?.ClientId == applicationSlug);

            if (orgApp?.Application == null) return null;

            var roles = (await apiClient.RolesAllAsync(orgApp.ApplicationId, ct)).ToList();

            // TODO: Get user assignments when endpoint is available to list all users for an org-app
            var userAssignments = new List<UserAssignmentResponse>();

            return ViewModelMapper.ToApplicationDetailsViewModel(org, orgApp, roles, userAssignments);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<DisableApplicationViewModel?> GetDisableApplicationViewModelAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var enabledApps = (await apiClient.ApplicationsAllAsync(org.Id, ct)).ToList();
            var orgApp = enabledApps.FirstOrDefault(oa => oa.Application?.ClientId == applicationSlug);
            
            if (orgApp?.Application == null) return null;

            // TODO: Get user assignments when endpoint is available
            var userAssignments = new List<UserAssignmentResponse>();

            return ViewModelMapper.ToDisableApplicationViewModel(org, orgApp, userAssignments);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<bool> DisableApplicationAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var enabledApps = (await apiClient.ApplicationsAllAsync(org.Id, ct)).ToList();
            var orgApp = enabledApps.FirstOrDefault(oa => oa.Application?.ClientId == applicationSlug);
            
            if (orgApp == null) return false;

            // Call API to disable the organisation-application relationship
            await apiClient.ApplicationsDELETEAsync(org.Id, orgApp.ApplicationId, ct);
            return true;
        }
        catch (ApiException)
        {
            return false;
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