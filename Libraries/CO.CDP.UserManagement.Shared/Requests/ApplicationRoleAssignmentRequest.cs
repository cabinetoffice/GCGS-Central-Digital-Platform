namespace CO.CDP.UserManagement.Shared.Requests;

public sealed record ApplicationRoleAssignmentRequest(
    int OrganisationApplicationId,
    int ApplicationId,
    bool GiveAccess,
    int? SelectedRoleId,
    IReadOnlyList<int>? SelectedRoleIds
);