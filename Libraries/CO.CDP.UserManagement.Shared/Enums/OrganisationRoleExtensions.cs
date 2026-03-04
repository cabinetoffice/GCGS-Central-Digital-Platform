namespace CO.CDP.UserManagement.Shared.Enums;

public static class OrganisationRoleExtensions
{
    public static string GetDescription(this OrganisationRole role) =>
        role switch
        {
            OrganisationRole.Member => "Can access assigned applications only. Cannot manage organisation settings or other users.",
            OrganisationRole.Admin => "Can add and remove users, enable applications for users, and assign users to applications. Cannot transfer ownership.",
            OrganisationRole.Owner => "Full control of the organisation including transferring ownership and deleting the organisation. An organisation must have at least one owner.",
            _ => string.Empty
        };
}
