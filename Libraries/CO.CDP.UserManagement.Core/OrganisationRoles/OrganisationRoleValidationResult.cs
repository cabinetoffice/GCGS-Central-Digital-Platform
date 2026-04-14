namespace CO.CDP.UserManagement.Core.OrganisationRoles;

public sealed record OrganisationRoleValidationResult(
    bool IsValid,
    string? ModelKey = null,
    string? ErrorMessage = null)
{
    public static OrganisationRoleValidationResult Success() => new(true);
    public static OrganisationRoleValidationResult Fail(string key, string message) => new(false, key, message);
}