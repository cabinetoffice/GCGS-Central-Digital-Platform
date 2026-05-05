namespace CO.CDP.UserManagement.App.Models;

public abstract record ApplicationRoleSubmitResult
{
    public sealed record NotFound : ApplicationRoleSubmitResult;
    public sealed record ValidationError(
        ChangeUserApplicationRolesViewModel ViewModel,
        IReadOnlyList<(string Key, string Message)> Errors) : ApplicationRoleSubmitResult;
    public sealed record Saved : ApplicationRoleSubmitResult;
}
