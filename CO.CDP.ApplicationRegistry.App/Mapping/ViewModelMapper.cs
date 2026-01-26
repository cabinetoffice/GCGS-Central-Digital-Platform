using CO.CDP.ApplicationRegistry.App.Models;
using CO.CDP.ApplicationRegistry.WebApiClient;

namespace CO.CDP.ApplicationRegistry.App.Mapping;

public static class ViewModelMapper
{
    public static HomeViewModel ToHomeViewModel(
        OrganisationResponse organisation,
        IReadOnlyList<OrganisationApplicationResponse> enabledApps,
        IReadOnlyList<RoleResponse> roles) =>
        new(
            OrganisationName: organisation.Name,
            Stats: CalculateStats(enabledApps, roles),
            EnabledApplications: enabledApps
                .Where(a => a.IsActive)
                .Select(ToEnabledApplicationViewModel)
                .ToList()
        );

    public static ApplicationsViewModel ToApplicationsViewModel(
        OrganisationResponse organisation,
        IReadOnlyList<ApplicationResponse> allApplications,
        IReadOnlyList<OrganisationApplicationResponse> enabledApps) =>
        new(
            OrganisationName: organisation.Name,
            EnabledApplications: enabledApps
                .Where(a => a.IsActive)
                .Select(oa => ToApplicationViewModel(oa, isEnabled: true))
                .ToList(),
            AvailableApplications: allApplications
                .Where(app => !enabledApps.Any(ea => ea.ApplicationId == app.Id && ea.IsActive))
                .Select(app => ToApplicationViewModel(app, isEnabled: false))
                .ToList()
        );

    public static EnabledApplicationViewModel ToEnabledApplicationViewModel(
        OrganisationApplicationResponse orgApp) =>
        new(
            Id: orgApp.Application?.Id ?? 0,
            Name: orgApp.Application?.Name ?? string.Empty,
            Description: string.Empty, // TODO: Add description to ApplicationSummaryResponse
            UsersAssigned: 0, // TODO: Calculate from user assignments
            RolesAvailable: 0 // TODO: Calculate from roles
        );

    public static ApplicationViewModel ToApplicationViewModel(
        OrganisationApplicationResponse orgApp,
        bool isEnabled) =>
        new(
            Id: orgApp.Application?.Id ?? 0,
            Slug: orgApp.Application?.ClientId ?? string.Empty, // TODO: Use slug from API when available
            Name: orgApp.Application?.Name ?? string.Empty,
            Description: string.Empty, // TODO: Add description to ApplicationSummaryResponse
            Category: string.Empty, // TODO: Add category to API
            IsEnabled: isEnabled,
            UsersAssigned: 0, // TODO: Calculate from user assignments
            RolesAvailable: 0 // TODO: Calculate from roles
        );

    public static ApplicationViewModel ToApplicationViewModel(
        ApplicationResponse app,
        bool isEnabled) =>
        new(
            Id: app.Id,
            Slug: app.ClientId, // TODO: Use slug from API when available
            Name: app.Name,
            Description: app.Description ?? string.Empty,
            Category: string.Empty, // TODO: Add category to API
            IsEnabled: isEnabled,
            UsersAssigned: 0,
            RolesAvailable: 0
        );

    private static DashboardStats CalculateStats(
        IReadOnlyList<OrganisationApplicationResponse> enabledApps,
        IReadOnlyList<RoleResponse> roles) =>
        new(
            ApplicationsEnabled: enabledApps.Count(a => a.IsActive),
            TotalUsers: 0, // TODO: Calculate from user memberships
            ActiveAssignments: 0, // TODO: Calculate from user assignments
            RolesAssigned: roles.Count
        );
}
