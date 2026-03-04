using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Models;

public sealed record OrganisationRoleOption(OrganisationRole Role, string Description);

public static class OrganisationRoleContent
{
    public static IReadOnlyList<OrganisationRoleOption> Options { get; } =
    [
        new(OrganisationRole.Member, OrganisationRole.Member.GetDescription()),
        new(OrganisationRole.Admin, OrganisationRole.Admin.GetDescription()),
        new(OrganisationRole.Owner, OrganisationRole.Owner.GetDescription())
    ];

    public static string GetTagClass(OrganisationRole role) =>
        role switch
        {
            OrganisationRole.Owner => "govuk-tag--blue",
            OrganisationRole.Admin => "govuk-tag--green",
            OrganisationRole.Member => "govuk-tag--orange",
            _ => string.Empty
        };

}
