namespace CO.CDP.OrganisationApp.Constants;

public class PersonScopes
{
    public const string SupportAdmin = "SUPPORTADMIN";
    public const string SuperAdmin = "SUPERADMIN";
}

public class PersonScopeRequirement
{
    public const string SupportAdmin = "PersonScope_" + PersonScopes.SupportAdmin;
}