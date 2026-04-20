namespace CO.CDP.UserManagement.App.Models;

public abstract record OrganisationRoleChangeResult
{
    public sealed record NotFound : OrganisationRoleChangeResult;
    public sealed record ValidationError(string ModelKey, string Message) : OrganisationRoleChangeResult;
    public sealed record Saved : OrganisationRoleChangeResult;
}
