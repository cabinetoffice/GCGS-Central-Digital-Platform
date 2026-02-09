namespace CO.CDP.UserManagement.App.Models;

public sealed record ApplicationsViewModel(
    string OrganisationName,
    IReadOnlyList<ApplicationViewModel> EnabledApplications,
    IReadOnlyList<ApplicationViewModel> AvailableApplications,
    IReadOnlyList<string> Categories,
    string? SelectedCategory = null,
    string? SelectedStatus = null,
    string? SearchTerm = null)
{
    public int TotalCount => EnabledApplications.Count + AvailableApplications.Count;

    public static ApplicationsViewModel Empty => new(
        OrganisationName: string.Empty,
        EnabledApplications: [],
        AvailableApplications: [],
        Categories: []);
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