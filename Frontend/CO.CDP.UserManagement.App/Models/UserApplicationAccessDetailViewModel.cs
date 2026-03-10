using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record UserApplicationAccessDetailViewModel(
    int ApplicationId,
    string ApplicationName,
    string? ApplicationDescription,
    IReadOnlyList<string> Permissions,
    DateTimeOffset AssignedDate,
    string AssignedByEmail,
    ApplicationRole ApplicationRole)
{
    public string RoleTagClass => ApplicationRole switch
    {
        ApplicationRole.Admin => "govuk-tag--green",
        ApplicationRole.Editor => "govuk-tag--blue",
        _ => string.Empty
    };
}
