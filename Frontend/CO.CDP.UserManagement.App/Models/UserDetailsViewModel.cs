using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record UserDetailsViewModel(
    string OrganisationName,
    string OrganisationSlug,
    Guid CdpPersonId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    OrganisationRole OrganisationRole,
    string MemberSince)
{
    public string OrganisationRoleTagClass => OrganisationRole switch
    {
        OrganisationRole.Owner => "govuk-tag--blue",
        OrganisationRole.Admin => "govuk-tag--green",
        OrganisationRole.Member => "govuk-tag--orange",
        _ => string.Empty
    };
}
