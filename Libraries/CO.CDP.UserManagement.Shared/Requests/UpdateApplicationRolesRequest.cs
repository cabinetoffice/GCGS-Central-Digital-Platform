namespace CO.CDP.UserManagement.Shared.Requests;

public sealed record UpdateApplicationRolesRequest(
    IReadOnlyList<ApplicationRoleAssignmentRequest> Applications
);