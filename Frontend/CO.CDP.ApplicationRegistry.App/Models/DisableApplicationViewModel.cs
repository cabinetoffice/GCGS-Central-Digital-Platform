namespace CO.CDP.ApplicationRegistry.App.Models;

public sealed record DisableApplicationViewModel(
    string OrganisationName,
    string OrganisationSlug,
    int ApplicationId,
    string ApplicationSlug,
    string ApplicationName,
    string? ApplicationDescription,
    int UsersAssigned,
    int ActiveAssignments)
{
    public static DisableApplicationViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationSlug: string.Empty,
        ApplicationId: 0,
        ApplicationSlug: string.Empty,
        ApplicationName: string.Empty,
        ApplicationDescription: null,
        UsersAssigned: 0,
        ActiveAssignments: 0);
}
