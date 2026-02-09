using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.ApplicationRegistry.WebApiClient;
using CO.CDP.UserManagement.App.Models;

namespace CO.CDP.UserManagement.App.Mapping;

public static class ViewModelMapper
{
    public static HomeViewModel ToHomeViewModel(
        OrganisationResponse organisation,
        IReadOnlyList<OrganisationApplicationResponse> enabledApps,
        IReadOnlyList<RoleResponse> roles,
        IReadOnlyList<OrganisationUserResponse> users) =>
        new(
            OrganisationName: organisation.Name,
            Stats: CalculateStats(enabledApps, roles, users),
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

    public static EnableApplicationViewModel ToEnableApplicationViewModel(
        OrganisationResponse organisation,
        ApplicationResponse application,
        IReadOnlyList<RoleResponse> roles) =>
        new(
            OrganisationName: organisation.Name,
            OrganisationSlug: organisation.Slug,
            ApplicationId: application.Id,
            ApplicationSlug: application.ClientId,
            ApplicationName: application.Name,
            ApplicationDescription: application.Description,
            ApplicationCategory: application.Category,
            SupportContact: null, // TODO: Add support contact when available
            AvailableRoles: roles.Select(ToRoleViewModel).ToList()
        );

    public static EnableApplicationSuccessViewModel ToEnableSuccessViewModel(
        OrganisationResponse organisation,
        ApplicationResponse application) =>
        new(
            OrganisationName: organisation.Name,
            OrganisationSlug: organisation.Slug,
            ApplicationName: application.Name,
            ApplicationSlug: application.ClientId,
            EnabledBy: "Current User", // TODO: Get current user when available
            EnabledAt: DateTimeOffset.UtcNow,
            AvailableRoles: new List<string>() // TODO: Get role names when available
        );

    public static ApplicationDetailsViewModel ToApplicationDetailsViewModel(
        OrganisationResponse organisation,
        OrganisationApplicationResponse orgApp,
        IReadOnlyList<RoleResponse> roles,
        IReadOnlyList<UserAssignmentResponse> userAssignments)
    {
        var app = orgApp.Application!;
        var activeAssignments = userAssignments.Where(ua => ua.IsActive).ToList();
        var totalPermissions = roles.SelectMany(r => r.Permissions ?? Enumerable.Empty<PermissionResponse>()).DistinctBy(p => p.Id).Count();

        return new(
            OrganisationName: organisation.Name,
            OrganisationSlug: organisation.Slug,
            ApplicationId: app.Id,
            ApplicationSlug: app.ClientId,
            ApplicationName: app.Name,
            ClientId: app.ClientId,
            ApplicationDescription: app.Description,
            ApplicationCategory: app.Category,
            SupportContact: null, // TODO: Add support contact when available
            IsEnabled: orgApp.IsActive,
            EnabledAt: orgApp.EnabledAt,
            EnabledBy: orgApp.EnabledBy,
            UsersAssigned: activeAssignments.Count,
            RolesAvailable: roles.Count,
            TotalPermissions: totalPermissions,
            Roles: roles.Select(ToRoleViewModel).ToList(),
            AssignedUsers: activeAssignments.Select(ToUserAssignmentViewModel).ToList()
        );
    }

    public static DisableApplicationViewModel ToDisableApplicationViewModel(
        OrganisationResponse organisation,
        OrganisationApplicationResponse orgApp,
        IReadOnlyList<UserAssignmentResponse> userAssignments)
    {
        var app = orgApp.Application!;
        var activeAssignments = userAssignments.Where(ua => ua.IsActive).ToList();

        return new(
            OrganisationName: organisation.Name,
            OrganisationSlug: organisation.Slug,
            ApplicationId: app.Id,
            ApplicationSlug: app.ClientId,
            ApplicationName: app.Name,
            ApplicationDescription: app.Description,
            UsersAssigned: activeAssignments.Count,
            ActiveAssignments: activeAssignments.Count
        );
    }

    private static Models.RoleViewModel ToRoleViewModel(RoleResponse role) =>
        new(
            Id: role.Id,
            Name: role.Name,
            Description: role.Description,
            Permissions: role.Permissions?.Select(p => p.Name).ToList() ?? new List<string>()
        );

    private static Models.UserAssignmentViewModel ToUserAssignmentViewModel(UserAssignmentResponse assignment)
    {
        var roleName = assignment.Roles?.FirstOrDefault()?.Name ?? string.Empty;

        return new(
            AssignmentId: assignment.Id,
            UserName: assignment.UserPrincipalId ?? "Unknown User", // TODO: Get actual user name when available
            UserEmail: assignment.UserPrincipalId ?? string.Empty, // TODO: Get actual email when available
            RoleName: roleName,
            AssignedAt: assignment.AssignedAt ?? assignment.CreatedAt,
            AssignedBy: assignment.AssignedBy
        );
    }

    private static DashboardStats CalculateStats(
        IReadOnlyList<OrganisationApplicationResponse> enabledApps,
        IReadOnlyList<RoleResponse> roles,
        IReadOnlyList<OrganisationUserResponse> users) =>
        new(
            ApplicationsEnabled: enabledApps.Count(a => a.IsActive),
            TotalUsers: users.Count(user => user.Status == UserStatus.Active),
            ActiveAssignments: 0, // TODO: Calculate from user assignments
            RolesAssigned: roles.Count
        );
}
