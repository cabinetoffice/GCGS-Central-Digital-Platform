using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Models;

public sealed record UserDetailsViewModel(
    OrganisationResponse? Organisation,
    Guid CdpPersonId,
    string FullName,
    string Email,
    OrganisationRole OrganisationRole,
    string MemberSince,
    IReadOnlyList<UserApplicationAccessDetailViewModel> ApplicationAccess)
{
    public Guid Id => CdpPersonId;

    public string OrganisationRoleTagClass => OrganisationRole switch
    {
        OrganisationRole.Owner => "govuk-tag--blue",
        OrganisationRole.Admin => "govuk-tag--green",
        OrganisationRole.Member => "govuk-tag--orange",
        _ => string.Empty
    };
}
