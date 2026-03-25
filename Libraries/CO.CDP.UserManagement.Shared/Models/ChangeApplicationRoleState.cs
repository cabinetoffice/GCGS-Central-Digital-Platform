namespace CO.CDP.UserManagement.Shared.Models;

public sealed record ChangeApplicationRoleState(
    string OrganisationSlug,
    Guid? CdpPersonId,
    Guid? InviteGuid,
    string UserDisplayName,
    string Email,
    IReadOnlyList<ApplicationRoleAssignmentState> Applications);

public sealed record ApplicationRoleAssignmentState(
    int OrganisationApplicationId,
    int ApplicationId,
    string ApplicationName,
    bool HasExistingAccess,
    bool GiveAccess,
    int? CurrentRoleId,
    string CurrentRoleName,
    int? SelectedRoleId,
    string SelectedRoleName,
    IReadOnlyList<int>? SelectedRoleIds = null,
    IReadOnlyList<int>? CurrentRoleIds = null); // Nullable for optionality