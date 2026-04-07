namespace CO.CDP.UserManagement.Core.ApplicationRoles;

public sealed record ApplicationRoleChangeInput(
    string OrganisationSlug,
    Guid? CdpPersonId,
    Guid? InviteGuid,
    string UserDisplayName,
    string? Email,
    IReadOnlyList<ApplicationRoleChangeInputItem> Applications);

public sealed record ApplicationRoleChangeInputItem(
    int OrganisationApplicationId,
    int ApplicationId,
    string ApplicationName,
    bool HasExistingAccess,
    bool GiveAccess,
    int? OriginalSingleRoleId,
    IReadOnlyList<int> OriginalMultiRoleIds,
    int? SelectedRoleId,
    IReadOnlyList<int> SelectedRoleIds,
    bool AllowsMultipleRoleAssignments,
    IReadOnlyList<ApplicationRoleOption> AvailableRoles);

public sealed record ApplicationRoleOption(int Id, string Name);
