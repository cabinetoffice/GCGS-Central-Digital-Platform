using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Models;

public sealed record InviteDetailsViewModel(
    OrganisationResponse? Organisation,
    Guid InviteGuid,
    int PendingInviteId,
    string FullName,
    string Email,
    OrganisationRole OrganisationRole,
    DateTimeOffset InvitedAt,
    IReadOnlyList<string> ApplicationNames)
{
    public string OrganisationRoleTagClass => OrganisationRole switch
    {
        OrganisationRole.Owner => "govuk-tag--blue",
        OrganisationRole.Admin => "govuk-tag--green",
        OrganisationRole.Member => "govuk-tag--orange",
        _ => string.Empty
    };
}
