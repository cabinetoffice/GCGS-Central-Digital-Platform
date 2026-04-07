using CO.CDP.UserManagement.Core.Common;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Core.Removal;

public static class UserRemovalValidator
{
    public static RemovalValidationResult Validate(
        string? targetEmail,
        string? currentUserEmail,
        OrganisationRole targetOrganisationRole,
        bool isLastOwner,
        OrganisationRole? currentUserOrganisationRole)
    {
        if (SelfServicePolicy.IsSelf(currentUserEmail, targetEmail))
            return RemovalValidationResult.Fail("You cannot remove yourself from the organisation.");

        if (currentUserOrganisationRole == OrganisationRole.Admin &&
            targetOrganisationRole == OrganisationRole.Owner)
            return RemovalValidationResult.Fail("You do not have permission to remove an Owner.");

        if (isLastOwner)
            return RemovalValidationResult.Fail("You cannot remove the last owner of the organisation.");

        return RemovalValidationResult.Success();
    }
}