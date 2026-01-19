namespace CO.CDP.ApplicationRegistry.App.Models;

public class HomeViewModel
{
    public required string OrganisationName { get; init; }
    public required DashboardStats Stats { get; init; }
    public required List<EnabledApplicationViewModel> EnabledApplications { get; init; }
}

public class DashboardStats
{
    public int ApplicationsEnabled { get; init; }
    public int TotalUsers { get; init; }
    public int ActiveAssignments { get; init; }
    public int RolesAssigned { get; init; }
}

public class EnabledApplicationViewModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public int UsersAssigned { get; init; }
    public int RolesAvailable { get; init; }
}
