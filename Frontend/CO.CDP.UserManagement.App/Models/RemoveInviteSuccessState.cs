using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed class RemoveInviteSuccessState
{
    public string OrganisationSlug { get; init; } = string.Empty;
    public string UserDisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string OrganisationName { get; init; } = string.Empty;
    public string MemberSince { get; init; } = string.Empty;
    public OrganisationRole Role { get; init; }
}
