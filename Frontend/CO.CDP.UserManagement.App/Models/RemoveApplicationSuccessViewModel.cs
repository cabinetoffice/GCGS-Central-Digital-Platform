namespace CO.CDP.UserManagement.App.Models;

public sealed class RemoveApplicationSuccessViewModel
{
    public string OrganisationSlug { get; init; } = string.Empty;

    public string UserDisplayName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string ApplicationName { get; init; } = string.Empty;

    public Guid CdpPersonId { get; init; }
}
