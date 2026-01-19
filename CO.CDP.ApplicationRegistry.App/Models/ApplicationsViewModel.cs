namespace CO.CDP.ApplicationRegistry.App.Models;

public class ApplicationsViewModel
{
    public required string OrganisationName { get; init; }
    public required List<ApplicationViewModel> EnabledApplications { get; init; }
    public required List<ApplicationViewModel> AvailableApplications { get; init; }
    public int TotalCount => EnabledApplications.Count + AvailableApplications.Count;
}

public class ApplicationViewModel
{
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public bool IsEnabled { get; init; }
    public int UsersAssigned { get; init; }
    public int RolesAvailable { get; init; }
}
