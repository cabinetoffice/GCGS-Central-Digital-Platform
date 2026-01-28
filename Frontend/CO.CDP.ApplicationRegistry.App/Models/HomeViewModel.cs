namespace CO.CDP.ApplicationRegistry.App.Models;

public sealed record HomeViewModel(
    string OrganisationName,
    DashboardStats Stats,
    IReadOnlyList<EnabledApplicationViewModel> EnabledApplications)
{
    public static HomeViewModel Empty => new(
        OrganisationName: string.Empty,
        Stats: DashboardStats.Empty,
        EnabledApplications: []);
}

public sealed record DashboardStats(
    int ApplicationsEnabled,
    int TotalUsers,
    int ActiveAssignments,
    int RolesAssigned)
{
    public static DashboardStats Empty => new(0, 0, 0, 0);
}

public sealed record EnabledApplicationViewModel(
    int Id,
    string Name,
    string Description,
    int UsersAssigned,
    int RolesAvailable);