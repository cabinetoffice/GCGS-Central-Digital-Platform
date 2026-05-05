using CO.CDP.UserManagement.App.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.UserManagement.App.Application.InviteUsers.Implementations;

/// <inheritdoc cref="IApplicationRoleSelectionMapper"/>
public sealed class ApplicationRoleSelectionMapper : IApplicationRoleSelectionMapper
{
    public ApplicationRolesStepViewModel MergePostedSelections(
        ApplicationRolesStepViewModel serverViewModel,
        ApplicationRolesStepPostModel posted)
    {
        var postedSelections = posted.Applications.ToDictionary(a => a.OrganisationApplicationId);

        var merged = serverViewModel.Applications
            .Select(application =>
            {
                if (postedSelections.TryGetValue(application.OrganisationApplicationId, out var postedSelection))
                {
                    var giveAccess = postedSelection.GiveAccess || application.IsEnabledByDefault;

                    if (application.AllowsMultipleRoleAssignments)
                    {
                        var selectedRoleIds = postedSelection.SelectedRoleIds;
                        return new ApplicationAccessSelectionViewModel
                        {
                            OrganisationApplicationId = application.OrganisationApplicationId,
                            ApplicationName = application.ApplicationName,
                            ApplicationDescription = application.ApplicationDescription,
                            AllowsMultipleRoleAssignments = true,
                            IsEnabledByDefault = application.IsEnabledByDefault,
                            GiveAccess = giveAccess,
                            SelectedRoleId = selectedRoleIds.Count > 0 ? selectedRoleIds[0] : null,
                            SelectedRoleIds = selectedRoleIds,
                            Roles = application.Roles
                        };
                    }

                    return new ApplicationAccessSelectionViewModel
                    {
                        OrganisationApplicationId = application.OrganisationApplicationId,
                        ApplicationName = application.ApplicationName,
                        ApplicationDescription = application.ApplicationDescription,
                        AllowsMultipleRoleAssignments = false,
                        IsEnabledByDefault = application.IsEnabledByDefault,
                        GiveAccess = giveAccess,
                        SelectedRoleId = postedSelection.SelectedRoleId,
                        SelectedRoleIds = application.SelectedRoleIds,
                        Roles = application.Roles
                    };
                }

                return new ApplicationAccessSelectionViewModel
                {
                    OrganisationApplicationId = application.OrganisationApplicationId,
                    ApplicationName = application.ApplicationName,
                    ApplicationDescription = application.ApplicationDescription,
                    AllowsMultipleRoleAssignments = application.AllowsMultipleRoleAssignments,
                    IsEnabledByDefault = application.IsEnabledByDefault,
                    GiveAccess = application.GiveAccess || application.IsEnabledByDefault,
                    SelectedRoleId = application.SelectedRoleId,
                    SelectedRoleIds = application.SelectedRoleIds,
                    Roles = application.Roles
                };
            })
            .ToList();

        return new ApplicationRolesStepViewModel
        {
            OrganisationId = serverViewModel.OrganisationId,
            FirstName = serverViewModel.FirstName,
            LastName = serverViewModel.LastName,
            Email = serverViewModel.Email,
            OrganisationRole = serverViewModel.OrganisationRole,
            Applications = merged
        };
    }

    public ApplicationRolesStepViewModel ApplyExistingSelections(
        ApplicationRolesStepViewModel serverViewModel,
        IReadOnlyList<InviteApplicationAssignment>? savedAssignments)
    {
        if (savedAssignments is null || savedAssignments.Count == 0)
            return serverViewModel;

        var savedByAppId = savedAssignments.ToDictionary(a => a.OrganisationApplicationId);

        var updated = serverViewModel.Applications
            .Select(application =>
            {
                if (!savedByAppId.TryGetValue(application.OrganisationApplicationId, out var saved))
                    return application;

                // Restore multi-role selections (ApplicationRoleIds) when present,
                // otherwise fall back to the single-role selection (ApplicationRoleId).
                var restoredRoleIds = saved.ApplicationRoleIds is { Count: > 0 }
                    ? saved.ApplicationRoleIds.ToList()
                    : saved.ApplicationRoleId > 0 ? [saved.ApplicationRoleId] : new List<int>();

                return new ApplicationAccessSelectionViewModel
                {
                    OrganisationApplicationId = application.OrganisationApplicationId,
                    ApplicationName = application.ApplicationName,
                    ApplicationDescription = application.ApplicationDescription,
                    AllowsMultipleRoleAssignments = application.AllowsMultipleRoleAssignments,
                    IsEnabledByDefault = application.IsEnabledByDefault,
                    GiveAccess = true,
                    SelectedRoleId = restoredRoleIds.Count > 0 ? restoredRoleIds[0] : null,
                    SelectedRoleIds = restoredRoleIds,
                    Roles = application.Roles
                };
            })
            .ToList();

        return new ApplicationRolesStepViewModel
        {
            OrganisationId = serverViewModel.OrganisationId,
            FirstName = serverViewModel.FirstName,
            LastName = serverViewModel.LastName,
            Email = serverViewModel.Email,
            OrganisationRole = serverViewModel.OrganisationRole,
            Applications = updated
        };
    }

    public bool ValidateSelections(ApplicationRolesStepViewModel merged, ModelStateDictionary modelState)
    {
        var selectedApplications = merged.Applications.Where(a => a.GiveAccess).ToList();

        if (selectedApplications.Count == 0)
            modelState.AddModelError("applicationSelections", "You must select at least one application and role");

        merged.Applications
            .Select((application, index) => new { application, index })
            .Where(item => item.application.GiveAccess && !HasRoleSelected(item.application))
            .ToList()
            .ForEach(item =>
                modelState.AddModelError(
                    $"Applications[{item.index}].SelectedRoleId",
                    $"Select a role for {item.application.ApplicationName}"));

        return modelState.IsValid;
    }

    public IReadOnlyList<InviteApplicationAssignment> MapToAssignments(
        IReadOnlyList<ApplicationAccessSelectionViewModel> selectedApps) =>
        selectedApps
            .Where(HasRoleSelected)
            .Select(a => new InviteApplicationAssignment
            {
                OrganisationApplicationId = a.OrganisationApplicationId,
                ApplicationRoleId = a.AllowsMultipleRoleAssignments
                    ? (a.SelectedRoleIds.Count > 0 ? a.SelectedRoleIds[0] : 0)
                    : a.SelectedRoleId.GetValueOrDefault(),
                ApplicationRoleIds = a.AllowsMultipleRoleAssignments ? a.SelectedRoleIds : null
            })
            .ToList();

    private static bool HasRoleSelected(ApplicationAccessSelectionViewModel app) =>
        app.AllowsMultipleRoleAssignments
            ? app.SelectedRoleIds.Count > 0
            : app.SelectedRoleId.HasValue && app.Roles.Any(r => r.Id == app.SelectedRoleId.Value);
}
