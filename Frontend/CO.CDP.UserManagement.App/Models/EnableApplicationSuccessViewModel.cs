namespace CO.CDP.UserManagement.App.Models;

public sealed record EnableApplicationSuccessViewModel(
    string OrganisationName,
    Guid OrganisationId,
    string ApplicationName,
    string ApplicationSlug,
    string EnabledBy,
    DateTimeOffset EnabledAt,
    IReadOnlyList<string> AvailableRoles)
{
    public static EnableApplicationSuccessViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationId: Guid.Empty,
        ApplicationName: string.Empty,
        ApplicationSlug: string.Empty,
        EnabledBy: string.Empty,
        EnabledAt: DateTimeOffset.UtcNow,
        AvailableRoles: []);
}
