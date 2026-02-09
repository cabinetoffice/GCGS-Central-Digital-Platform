namespace CO.CDP.UserManagement.App.Models;

public sealed record EnableApplicationViewModel(
    string OrganisationName,
    string OrganisationSlug,
    int ApplicationId,
    string ApplicationSlug,
    string ApplicationName,
    string? ApplicationDescription,
    string? ApplicationCategory,
    string? SupportContact,
    IReadOnlyList<RoleViewModel> AvailableRoles)
{
    public static EnableApplicationViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationSlug: string.Empty,
        ApplicationId: 0,
        ApplicationSlug: string.Empty,
        ApplicationName: string.Empty,
        ApplicationDescription: null,
        ApplicationCategory: null,
        SupportContact: null,
        AvailableRoles: []);
}

public sealed record RoleViewModel(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions);
