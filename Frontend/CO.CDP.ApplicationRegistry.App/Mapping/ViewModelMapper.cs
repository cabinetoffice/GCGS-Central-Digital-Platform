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
        IReadOnlyList<OrganisationApplicationResponse> enabledApps,
        string? selectedCategory = null,
        string? selectedStatus = null,
        string? searchTerm = null)
    {
        var enabledList = enabledApps
            .Where(a => a.IsActive)
            .Select(oa => ToApplicationViewModel(oa, isEnabled: true))
            .ToList();

        var availableList = allApplications
            .Where(app => !enabledApps.Any(ea => ea.ApplicationId == app.Id && ea.IsActive))
            .Select(app => ToApplicationViewModel(app, isEnabled: false))
            .ToList();

        // Get all categories before filtering (for dropdown)
        var allAppsUnfiltered = enabledList.Concat(availableList).ToList();
        var categories = allAppsUnfiltered
            .Where(a => !string.IsNullOrEmpty(a.Category))
            .Select(a => a.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            enabledList = enabledList.Where(a =>
                a.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(a.Description) && a.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))).ToList();
            availableList = availableList.Where(a =>
                a.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(a.Description) && a.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        if (!string.IsNullOrEmpty(selectedCategory))
        {
            enabledList = enabledList.Where(a => a.Category == selectedCategory).ToList();
            availableList = availableList.Where(a => a.Category == selectedCategory).ToList();
        }

        if (!string.IsNullOrEmpty(selectedStatus))
        {
            if (selectedStatus == "enabled")
            {
                availableList = new List<ApplicationViewModel>();
            }
            else if (selectedStatus == "available")
            {
                enabledList = new List<ApplicationViewModel>();
            }
        }

        return new(
            OrganisationName: organisation.Name,
            EnabledApplications: enabledList,
            AvailableApplications: availableList,
            Categories: categories,
            SelectedCategory: selectedCategory,
            SelectedStatus: selectedStatus,
            SearchTerm: searchTerm
        );
    }

    public static EnabledApplicationViewModel ToEnabledApplicationViewModel(
        OrganisationApplicationResponse orgApp) =>
        new(
            Id: orgApp.Application?.Id ?? 0,
            Slug: orgApp.Application?.ClientId ?? string.Empty,
            Name: orgApp.Application?.Name ?? string.Empty,
            Description: orgApp.Application?.Description ?? string.Empty,
            UsersAssigned: 0, // TODO: Calculate from user assignments
            RolesAvailable: 0 // TODO: Calculate from roles
        );

    public static ApplicationViewModel ToApplicationViewModel(
        OrganisationApplicationResponse orgApp,
        bool isEnabled) =>
        new(
            Id: orgApp.Application?.Id ?? 0,
            Slug: orgApp.Application?.ClientId ?? string.Empty,
            Name: orgApp.Application?.Name ?? string.Empty,
            Description: orgApp.Application?.Description ?? string.Empty,
            Category: orgApp.Application?.Category ?? string.Empty,
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
            Category: app.Category ?? string.Empty,
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
