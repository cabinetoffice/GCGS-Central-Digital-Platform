using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record UserApplicationAccessDetailViewModel(
    int ApplicationId,
    string ApplicationClientId,
    string ApplicationName,
    string? ApplicationDescription,
    IReadOnlyList<string> Permissions,
    DateTimeOffset AssignedDate,
    string AssignedByEmail,
    string ApplicationRole)
{
}
