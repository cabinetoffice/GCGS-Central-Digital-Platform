using CO.CDP.UserManagement.App.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.UserManagement.App.Application.InviteUsers;

/// <summary>
/// Pure mapper for the invite wizard's application-role selection step.
/// Contains no I/O dependencies — all methods transform data in memory.
/// </summary>
public interface IApplicationRoleSelectionMapper
{
    /// <summary>
    /// Merges the posted form selections onto the server-fetched view model,
    /// preserving application metadata (names, descriptions, available roles)
    /// while applying the user's choices (GiveAccess, SelectedRoleId/s).
    /// </summary>
    ApplicationRolesStepViewModel MergePostedSelections(
        ApplicationRolesStepViewModel serverViewModel,
        ApplicationRolesStepPostModel posted);

    /// <summary>
    /// Restores previously saved application assignments onto the server-fetched
    /// view model when the user navigates back to this step.
    /// Correctly restores both single-role (<c>SelectedRoleId</c>) and
    /// multi-role (<c>SelectedRoleIds</c>) selections.
    /// </summary>
    ApplicationRolesStepViewModel ApplyExistingSelections(
        ApplicationRolesStepViewModel serverViewModel,
        IReadOnlyList<InviteApplicationAssignment>? savedAssignments);

    /// <summary>
    /// Validates that at least one application is selected and that every selected
    /// application has a role assigned. Adds model errors to <paramref name="modelState"/>
    /// on failure.
    /// </summary>
    /// <returns><c>true</c> if the selections are valid; otherwise <c>false</c>.</returns>
    bool ValidateSelections(ApplicationRolesStepViewModel merged, ModelStateDictionary modelState);

    /// <summary>
    /// Maps the validated, selected applications to <see cref="InviteApplicationAssignment"/> records
    /// suitable for storage in <see cref="InviteUserState"/>.
    /// </summary>
    IReadOnlyList<InviteApplicationAssignment> MapToAssignments(
        IReadOnlyList<ApplicationAccessSelectionViewModel> selectedApps);
}
