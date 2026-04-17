using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Models;

public sealed record UsersViewModel(
    string OrganisationName,
    Guid OrganisationId,
    Guid? OrganisationGuid,
    IReadOnlyList<UserSummaryViewModel> Users,
    IReadOnlyList<ApplicationViewModel> AvailableApplications,
    string? SelectedRole,
    string? SelectedApplication,
    string? SearchTerm,
    int TotalCount,
    IReadOnlyList<JoinRequestResponse>? PendingJoinRequests = null)
{
    public bool HasActiveFilters =>
        !string.IsNullOrEmpty(SelectedRole) ||
        !string.IsNullOrEmpty(SelectedApplication) ||
        !string.IsNullOrEmpty(SearchTerm);

    public static UsersViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationId: Guid.Empty,
        OrganisationGuid: null,
        Users: [],
        AvailableApplications: [],
        SelectedRole: null,
        SelectedApplication: null,
        SearchTerm: null,
        TotalCount: 0,
        PendingJoinRequests: null);
}

public sealed record UserSummaryViewModel(
    Guid? Id,
    Guid? InviteGuid,
    string Name,
    string Email,
    OrganisationRole? OrganisationRole,
    UserStatus Status,
    IReadOnlyList<UserApplicationAccessViewModel> ApplicationAccess);

public sealed record UserApplicationAccessViewModel(
    string ApplicationName,
    string ApplicationSlug,
    string RoleName);