using CO.CDP.UserManagement.Core.Common;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Core.OrganisationRoles;

public static class OrganisationRoleChangeValidator
{
    public static OrganisationRoleValidationResult Validate(
        OrganisationRole? selectedRole,
        OrganisationRole currentRole,
        string? targetEmail,
        string? currentUserEmail,
        OrganisationRole? currentUserOrganisationRole)
    {
        if (selectedRole is null)
            return OrganisationRoleValidationResult.Fail(
                "organisationRole", "Select an organisation role");

        if (SelfServicePolicy.IsSelf(currentUserEmail, targetEmail))
            return OrganisationRoleValidationResult.Fail(
                "organisationRole", "You cannot change your own organisation role.");

        if (currentUserOrganisationRole == OrganisationRole.Admin &&
            currentRole == OrganisationRole.Owner)
            return OrganisationRoleValidationResult.Fail(
                "organisationRole", "You do not have permission to change an Owner's role.");

        if (selectedRole == currentRole)
            return OrganisationRoleValidationResult.Fail(
                "organisationRole", "Select a different role to continue");

        return OrganisationRoleValidationResult.Success();
    }
}