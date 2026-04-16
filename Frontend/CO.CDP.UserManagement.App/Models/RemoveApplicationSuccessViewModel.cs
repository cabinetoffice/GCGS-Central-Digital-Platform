namespace CO.CDP.UserManagement.App.Models;

public sealed class RemoveApplicationSuccessViewModel
{
    public Guid OrganisationId { get; init; } = Guid.Empty;

    public string UserDisplayName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string ApplicationName { get; init; } = string.Empty;

    public Guid CdpPersonId { get; init; }
}
