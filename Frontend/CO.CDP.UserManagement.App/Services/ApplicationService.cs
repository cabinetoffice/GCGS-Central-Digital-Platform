using CO.CDP.UserManagement.WebApiClient;
using CO.CDP.UserManagement.App.Mapping;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.Functional;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Services;

public sealed class ApplicationService(
    UserManagementClient apiClient) : IApplicationService
{
    public async Task<HomeViewModel?> GetHomeViewModelAsync(string organisationSlug, CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var apps = (await apiClient.ApplicationsAllAsync(org.Id, ct)).ToList();
            var roles = await GetAllRolesForApplicationsAsync(apps, ct);
            var users = (await apiClient.UsersAll2Async(org.CdpOrganisationGuid, ct)).ToList();
            return ViewModelMapper.ToHomeViewModel(org, apps, roles, users);
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

    public async Task<Result<ServiceFailure, ServiceOutcome>> EnableApplicationAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var allApps = (await apiClient.ApplicationsAllAsync(ct)).ToList();
            var app = allApps.FirstOrDefault(a => a.ClientId == applicationSlug);

            if (app == null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            var request = new EnableApplicationRequest { ApplicationId = app.Id };
            await apiClient.ApplicationsPOSTAsync(org.Id, request, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
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

    public async Task<Result<ServiceFailure, ServiceOutcome>> DisableApplicationAsync(
        string organisationSlug,
        string applicationSlug,
        CancellationToken ct = default)
    {
        try
        {
            var org = await apiClient.BySlugAsync(organisationSlug, ct);
            var enabledApps = (await apiClient.ApplicationsAllAsync(org.Id, ct)).ToList();
            var orgApp = enabledApps.FirstOrDefault(oa => oa.Application?.ClientId == applicationSlug);

            if (orgApp == null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

            await apiClient.ApplicationsDELETEAsync(org.Id, orgApp.ApplicationId, ct);
            return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);
        }
        catch (ApiException ex)
        {
            return ServiceResultMapper.FromApiException(ex);
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
