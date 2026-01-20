using System.Net;
using CO.CDP.ApplicationRegistry.App.Api;
using CO.CDP.ApplicationRegistry.App.Mapping;
using CO.CDP.ApplicationRegistry.App.Models;
using Refit;

namespace CO.CDP.ApplicationRegistry.App.Services;

public sealed class ApplicationService(
    IApplicationsApi applicationsApi,
    IOrganisationsApi organisationsApi,
    IOrganisationApplicationsApi organisationApplicationsApi) : IApplicationService
{
    public async Task<HomeViewModel?> GetHomeViewModelAsync(int orgId, CancellationToken ct = default)
    {
        try
        {
            var org = await organisationsApi.OrganisationsGet(orgId, ct);
            var apps = await organisationApplicationsApi.ApplicationsGet(orgId, ct);
            var roles = await GetAllRolesForApplicationsAsync(apps, ct);
            return ViewModelMapper.ToHomeViewModel(org, apps, roles);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<ApplicationsViewModel?> GetApplicationsViewModelAsync(int orgId, CancellationToken ct = default)
    {
        try
        {
            var org = await organisationsApi.OrganisationsGet(orgId, ct);
            var allApps = await applicationsApi.ApplicationsGet(ct);
            var enabledApps = await organisationApplicationsApi.ApplicationsGet(orgId, ct);
            return ViewModelMapper.ToApplicationsViewModel(org, allApps, enabledApps);
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
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
                var roles = await applicationsApi.RolesGet(app.ApplicationId, ct);
                allRoles.AddRange(roles);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Application not found, skip
            }
        }

        return allRoles;
    }
}
