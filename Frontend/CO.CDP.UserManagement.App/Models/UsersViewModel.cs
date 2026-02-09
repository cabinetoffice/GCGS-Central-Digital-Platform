using CO.CDP.ApplicationRegistry.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record UsersViewModel(
    string OrganisationName,
    string OrganisationSlug,
    IReadOnlyList<UserSummaryViewModel> Users,
    string? SelectedRole,
    string? SelectedApplication,
    string? SearchTerm,
    int TotalCount)
{
    public static UsersViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationSlug: string.Empty,
        Users: [],
        SelectedRole: null,
        SelectedApplication: null,
        SearchTerm: null,
        TotalCount: 0);
}

public sealed record UserSummaryViewModel(
    Guid? Id,
    string Name,
    string Email,
    OrganisationRole OrganisationRole,
    UserStatus Status,
    IReadOnlyList<UserApplicationAccessViewModel> ApplicationAccess);

public sealed record UserApplicationAccessViewModel(
    string ApplicationName,
    string ApplicationSlug,
    string RoleName);
