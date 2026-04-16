using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;

namespace CO.CDP.UserManagement.App.Models;

public sealed record OrganisationRoleOption(OrganisationRole Role, string DisplayName, string Description);

public static class OrganisationRoleOptionMapper
{
    public static IReadOnlyList<OrganisationRoleOption> ToOptions(this IEnumerable<OrganisationRoleDefinitionResponse> roles) =>
        roles.Select(role => new OrganisationRoleOption(
                role.Id,
                role.DisplayName,
                role.Description ?? string.Empty))
            .ToList();
}

public sealed record OrganisationRoleStepViewModel(
    Guid OrganisationId,
    string FirstName,
    string LastName,
    string Email,
    OrganisationRole SelectedRole,
    bool ReturnToCheckAnswers,
    IReadOnlyList<OrganisationRoleOption> RoleOptions);

public sealed record ChangeUserRolePageViewModel(
    string OrganisationName,
    Guid OrganisationId,
    string UserDisplayName,
    string Email,
    OrganisationRole CurrentRole,
    OrganisationRole? SelectedRole,
    bool IsPending,
    Guid? CdpPersonId,
    Guid? InviteGuid,
    IReadOnlyList<OrganisationRoleOption> RoleOptions)
{
    public static ChangeUserRolePageViewModel From(
        ChangeUserRoleViewModel model,
        IReadOnlyList<OrganisationRoleOption> roleOptions,
        OrganisationRole? selectedRole = null) =>
        new(
            model.OrganisationName,
            model.OrganisationId,
            model.UserDisplayName,
            model.Email,
            model.CurrentRole,
            selectedRole ?? model.SelectedRole,
            model.IsPending,
            model.CdpPersonId,
            model.InviteGuid,
            roleOptions);
}

public static class OrganisationRoleContent
{
    public static string GetTagClass(OrganisationRole role) =>
        role switch
        {
            OrganisationRole.Owner => "govuk-tag--blue",
            OrganisationRole.Admin => "govuk-tag--green",
            OrganisationRole.Member => "govuk-tag--orange",
            _ => string.Empty
        };

}
