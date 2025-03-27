namespace CO.CDP.OrganisationApp.Constants;

public class OrganisationPersonScopes
{
    public const string AdminOrSupportAdmin = "ADMINORSUPPORTADMIN";
    public const string Admin = "ADMIN";
    public const string Editor = "EDITOR";
    public const string Viewer = "VIEWER";
    public const string Responder = "RESPONDER";
}

public class OrgScopeRequirement
{
    public const string Admin = "OrgScope_" + OrganisationPersonScopes.Admin;
    public const string Editor = "OrgScope_" + OrganisationPersonScopes.Editor;
    public const string Viewer = "OrgScope_" + OrganisationPersonScopes.Viewer;
}

public class OrgScopeWithSupportAdminBypassRequirement
{
    public const string AdminOrSupportAdmin = "OrgScopeSupportAdminBypass_" + OrganisationPersonScopes.AdminOrSupportAdmin;
}