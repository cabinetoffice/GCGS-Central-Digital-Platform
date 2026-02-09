namespace CO.CDP.UserManagement.App.Models;

public sealed record EnableApplicationSuccessViewModel(
    string OrganisationName,
    string OrganisationSlug,
    string ApplicationName,
    string ApplicationSlug,
    string EnabledBy,
    DateTimeOffset EnabledAt,
    IReadOnlyList<string> AvailableRoles)
{
    public static EnableApplicationSuccessViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationSlug: string.Empty,
        ApplicationName: string.Empty,
        ApplicationSlug: string.Empty,
        EnabledBy: string.Empty,
        EnabledAt: DateTimeOffset.UtcNow,
        AvailableRoles: []);
}
