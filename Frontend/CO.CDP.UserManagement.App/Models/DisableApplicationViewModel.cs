namespace CO.CDP.UserManagement.App.Models;

public sealed record DisableApplicationViewModel(
    string OrganisationName,
    Guid OrganisationId,
    int ApplicationId,
    string ApplicationSlug,
    string ApplicationName,
    string? ApplicationDescription,
    bool IsEnabledByDefault,
    int UsersAssigned,
    int ActiveAssignments)
{
    public static DisableApplicationViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationId: Guid.Empty,
        ApplicationId: 0,
        ApplicationSlug: string.Empty,
        ApplicationName: string.Empty,
        ApplicationDescription: null,
        IsEnabledByDefault: false,
        UsersAssigned: 0,
        ActiveAssignments: 0);
}
