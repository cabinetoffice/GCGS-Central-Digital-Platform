namespace CO.CDP.UserManagement.App.Models;

public sealed record ChangeApplicationRoleState(
    string OrganisationSlug,
    Guid? CdpPersonId,
    Guid? InviteGuid,
    string UserDisplayName,
    string Email,
    IReadOnlyList<ApplicationRoleAssignmentState> Applications);

public sealed record ApplicationRoleAssignmentState(
    int OrganisationApplicationId,
    string ApplicationName,
    int? CurrentRoleId,
    string CurrentRoleName,
    int? SelectedRoleId,
    string SelectedRoleName);
