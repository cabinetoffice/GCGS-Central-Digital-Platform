namespace CO.CDP.UserManagement.Core.ApplicationRoles;

public static class ApplicationRoleChangePlanner
{
    public static ApplicationRolePlanResult Plan(ApplicationRoleChangeInput input)
    {
        var errors = new List<(string Key, string Message)>();

        // Pass 1: validate that every granted app has a role selected
        var roleErrors = input.Applications
            .Select((app, i) => (app, i))
            .Where(x => x.app.GiveAccess && !HasRoleSelected(x.app))
            .Select(x => (
                Key: $"Applications[{x.i}].SelectedRoleId",
                Message: $"Select a role for {x.app.ApplicationName}"))
            .ToList();

        errors.AddRange(roleErrors);

        // Pass 2: validate that something has actually changed
        if (roleErrors.Count == 0)
        {
            var anyNewAccess = input.Applications.Any(a => !a.HasExistingAccess && a.GiveAccess);
            var anyRoleChanged = input.Applications.Any(a => a.HasExistingAccess && HasRoleChanged(a));

            if (!anyNewAccess && !anyRoleChanged)
                errors.Add((
                    Key: "Applications",
                    Message: "Select a different role or grant access to at least one application to continue"));
        }

        if (errors.Count > 0)
            return ApplicationRolePlanResult.Fail(errors);

        // Build output — only include apps with existing or newly-granted access
        var assignments = input.Applications
            .Where(a => a.HasExistingAccess || a.GiveAccess)
            .Select(a =>
            {
                var newRoleIds = ResolveNewRoleIds(a);
                var currentRoleIds = ResolveCurrentRoleIds(a);

                return new ApplicationRoleAssignmentOutput(
                    OrganisationApplicationId: a.OrganisationApplicationId,
                    ApplicationId: a.ApplicationId,
                    ApplicationName: a.ApplicationName,
                    HasExistingAccess: a.HasExistingAccess,
                    GiveAccess: a.GiveAccess,
                    CurrentSingleRoleId: currentRoleIds.Count > 0 ? currentRoleIds[0] : a.OriginalSingleRoleId,
                    CurrentRoleIds: currentRoleIds.Count > 0 ? currentRoleIds : null,
                    CurrentRoleName: FormatRoleNames(currentRoleIds, a.OriginalSingleRoleId, a.AvailableRoles,
                        a.AllowsMultipleRoleAssignments),
                    SelectedSingleRoleId: newRoleIds.Count > 0 ? newRoleIds[0] : null,
                    SelectedRoleIds: newRoleIds,
                    SelectedRoleName: FormatRoleNames(newRoleIds, a.SelectedRoleId, a.AvailableRoles,
                        a.AllowsMultipleRoleAssignments));
            })
            .ToList();

        return ApplicationRolePlanResult.Success(new ApplicationRolePlanOutput(
            OrganisationSlug: input.OrganisationSlug,
            CdpPersonId: input.CdpPersonId,
            InviteGuid: input.InviteGuid,
            UserDisplayName: input.UserDisplayName,
            Email: input.Email,
            Assignments: assignments));
    }

    private static bool HasRoleSelected(ApplicationRoleChangeInputItem app) =>
        app.AllowsMultipleRoleAssignments
            ? app.SelectedRoleIds.Count > 0
            : app.SelectedRoleId.HasValue;

    private static bool HasRoleChanged(ApplicationRoleChangeInputItem app)
    {
        var selected = ResolveNewRoleIds(app).OrderBy(x => x);
        var current = ResolveCurrentRoleIds(app).OrderBy(x => x);
        return !selected.SequenceEqual(current);
    }

    private static IReadOnlyList<int> ResolveNewRoleIds(ApplicationRoleChangeInputItem app) =>
        app.AllowsMultipleRoleAssignments
            ? app.SelectedRoleIds
            : app.SelectedRoleId.HasValue
                ? [app.SelectedRoleId.Value]
                : [];

    private static IReadOnlyList<int> ResolveCurrentRoleIds(ApplicationRoleChangeInputItem app) =>
        app.OriginalMultiRoleIds.Count > 0
            ? app.OriginalMultiRoleIds
            : app.OriginalSingleRoleId.HasValue
                ? [app.OriginalSingleRoleId.Value]
                : [];

    private static string FormatRoleNames(
        IReadOnlyList<int> roleIds,
        int? fallbackSingleId,
        IReadOnlyList<ApplicationRoleOption> availableRoles,
        bool allowsMultiple)
    {
        if (allowsMultiple)
        {
            return string.Join(", ", roleIds
                .Select(id => availableRoles.FirstOrDefault(r => r.Id == id)?.Name)
                .Where(n => n is not null));
        }

        var id = roleIds.Count > 0 ? roleIds[0] : fallbackSingleId;
        return availableRoles.FirstOrDefault(r => r.Id == id)?.Name ?? string.Empty;
    }
}