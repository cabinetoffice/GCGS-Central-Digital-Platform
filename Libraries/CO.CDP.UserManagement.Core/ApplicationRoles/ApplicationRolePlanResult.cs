namespace CO.CDP.UserManagement.Core.ApplicationRoles;

public sealed record ApplicationRolePlanResult(
    bool IsValid,
    IReadOnlyList<(string Key, string Message)> Errors,
    ApplicationRolePlanOutput? Output = null)
{
    public static ApplicationRolePlanResult Success(ApplicationRolePlanOutput output) =>
        new(true, Array.Empty<(string, string)>(), output);

    public static ApplicationRolePlanResult Fail(IReadOnlyList<(string Key, string Message)> errors) =>
        new(false, errors);
}