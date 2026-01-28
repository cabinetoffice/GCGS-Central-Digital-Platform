namespace CO.CDP.ApplicationRegistry.App.Models;

public sealed record ApplicationsViewModel(
    string OrganisationName,
    IReadOnlyList<ApplicationViewModel> EnabledApplications,
    IReadOnlyList<ApplicationViewModel> AvailableApplications)
{
    public int TotalCount => EnabledApplications.Count + AvailableApplications.Count;

    public static ApplicationsViewModel Empty => new(
        OrganisationName: string.Empty,
        EnabledApplications: [],
        AvailableApplications: []);
}

public sealed record ApplicationViewModel(
    int Id,
    string Slug,
    string Name,
    string Description,
    string Category,
    bool IsEnabled,
    int UsersAssigned,
    int RolesAvailable);