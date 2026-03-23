namespace CO.CDP.UserManagement.App.Models;

public sealed record RevokeApplicationAccessViewModel(
    string OrganisationSlug,
    string UserDisplayName,
    string UserEmail,
    string ApplicationName,
    string ApplicationSlug,
    int AssignmentId,
    int OrgId,
    string UserPrincipalId,
    string RoleName,
    DateTimeOffset? AssignedAt,
    string? AssignedByName)
{
    public static RevokeApplicationAccessViewModel Empty => new(
        OrganisationSlug: string.Empty,
        UserDisplayName: string.Empty,
        UserEmail: string.Empty,
        ApplicationName: string.Empty,
        ApplicationSlug: string.Empty,
        AssignmentId: 0,
        OrgId: 0,
        UserPrincipalId: string.Empty,
        RoleName: string.Empty,
        AssignedAt: null,
        AssignedByName: null);
}

public sealed record RevokeApplicationAccessSuccessViewModel(
    string OrganisationSlug,
    string UserDisplayName,
    string ApplicationName);

