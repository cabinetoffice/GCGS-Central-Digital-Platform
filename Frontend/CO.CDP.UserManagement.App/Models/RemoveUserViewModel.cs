using System.ComponentModel.DataAnnotations;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record RemoveUserViewModel(
    string OrganisationName = "",
    string OrganisationSlug = "",
    string UserDisplayName = "",
    string Email = "",
    OrganisationRole CurrentRole = OrganisationRole.Member,
    string MemberSinceFormatted = "",
    Guid? CdpPersonId = null,
    int? PendingInviteId = null,
    [Required(ErrorMessage = "Select if you want to remove this user")]
    bool? RemoveConfirmed = null)
{
    public static RemoveUserViewModel Empty => new(
        OrganisationName: string.Empty,
        OrganisationSlug: string.Empty,
        UserDisplayName: string.Empty,
        Email: string.Empty,
        CurrentRole: OrganisationRole.Member,
        MemberSinceFormatted: string.Empty,
        CdpPersonId: null,
        PendingInviteId: null,
        RemoveConfirmed: null);
}
