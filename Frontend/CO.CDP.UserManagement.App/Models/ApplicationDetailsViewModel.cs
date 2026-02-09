namespace CO.CDP.UserManagement.App.Models;

public sealed record ApplicationDetailsViewModel(
    string OrganisationName,
    string OrganisationSlug,
    int ApplicationId,
    string ApplicationSlug,
    string ApplicationName,
    string ClientId,
    string? ApplicationDescription,
    string? ApplicationCategory,
    string? SupportContact,
    bool IsEnabled,
    DateTimeOffset? EnabledAt,
    string? EnabledBy,
    int UsersAssigned,
    int RolesAvailable,
    int TotalPermissions,
    IReadOnlyList<RoleViewModel> Roles,
    IReadOnlyList<UserAssignmentViewModel> AssignedUsers)
{
    public static ApplicationDetailsViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationSlug: string.Empty,
        ApplicationId: 0,
        ApplicationSlug: string.Empty,
        ApplicationName: string.Empty,
        ClientId: string.Empty,
        ApplicationDescription: null,
        ApplicationCategory: null,
        SupportContact: null,
        IsEnabled: false,
        EnabledAt: null,
        EnabledBy: null,
        UsersAssigned: 0,
        RolesAvailable: 0,
        TotalPermissions: 0,
        Roles: [],
        AssignedUsers: []);
}

public sealed record UserAssignmentViewModel(
    int AssignmentId,
    string UserName,
    string UserEmail,
    string RoleName,
    DateTimeOffset AssignedAt,
    string? AssignedBy);
