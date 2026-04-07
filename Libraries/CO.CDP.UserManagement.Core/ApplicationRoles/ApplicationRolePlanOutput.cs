namespace CO.CDP.UserManagement.Core.ApplicationRoles;

public sealed record ApplicationRolePlanOutput(
    string OrganisationSlug,
    Guid? CdpPersonId,
    Guid? InviteGuid,
    string UserDisplayName,
    string? Email,
    IReadOnlyList<ApplicationRoleAssignmentOutput> Assignments);

public sealed record ApplicationRoleAssignmentOutput(
    int OrganisationApplicationId,
    int ApplicationId,
    string ApplicationName,
    bool HasExistingAccess,
    bool GiveAccess,
    int? CurrentSingleRoleId,
    IReadOnlyList<int>? CurrentRoleIds,
    string CurrentRoleName,
    int? SelectedSingleRoleId,
    IReadOnlyList<int> SelectedRoleIds,
    string SelectedRoleName);