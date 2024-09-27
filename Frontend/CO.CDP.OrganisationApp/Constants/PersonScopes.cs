namespace CO.CDP.OrganisationApp.Constants;

public class PersonScopes
{
    public const string SupportAdmin = "SUPPORTADMIN";
}

public class ScopeRequirement
{
    public const string SupportAdmin = "Scope_" + PersonScopes.SupportAdmin;
}