namespace CO.CDP.UserManagement.App.Models;

public sealed record ChangeApplicationRoleState(
    Guid OrganisationId,
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
    IReadOnlyList<int>? CurrentRoleIds = null);
