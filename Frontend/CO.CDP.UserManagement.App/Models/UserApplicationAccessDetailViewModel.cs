namespace CO.CDP.UserManagement.App.Models;

public sealed record UserApplicationAccessDetailViewModel(
    int ApplicationId,
    string ApplicationName,
    string? ApplicationDescription,
    IReadOnlyList<string> Permissions,
    DateTimeOffset AssignedDate,
    string AssignedByEmail,
    string ApplicationRole)
{
    public string RoleTagClass => ApplicationRole switch
    {
        "Admin" => "govuk-tag--green",
        "Editor" => "govuk-tag--blue",
        _ => string.Empty
    };
}
