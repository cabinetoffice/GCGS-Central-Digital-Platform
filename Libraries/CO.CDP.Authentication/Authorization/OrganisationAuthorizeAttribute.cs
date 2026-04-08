using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.Authorization;

/*
 * Examples how the attribute parameters are converted to policy name
 *
 * [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey], ["ADMIN", "VIEWER"], OrganisationIdLocation.Path)]
 * Org_Channels$OneLogin|ServiceKey;OrgScopes$ADMIN|VIEWER;OrgIdLoc$Path;
 *
 * [OrganisationAuthorize([AuthenticationChannel.ServiceKey])]
 * Org_Channels$ServiceKey;OrgScopes$;OrgIdLoc$None;
 *
 * With application scope:
 * [OrganisationAuthorize([AuthenticationChannel.OneLogin], organisationIdLocation: OrganisationIdLocation.Path, applicationClientId: "find-a-tender", applicationRoles: ["DataManager"])]
 * Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$Path;PersonScopes$;ApiKeyScopes$;AppClientId$find-a-tender;AppRoles$DataManager;AppPerms$;
 */
public class OrganisationAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Org_";
    public const string ChannelsGroup = "Channels";
    public const string OrganisationPersonScopesGroup = "OrgScopes";
    public const string OrgIdLocGroup = "OrgIdLoc";
    public const string PersonScopesGroup = "PersonScopes";
    public const string ApiKeyScopesGroup = "ApiKeyScopes";
    public const string AppClientIdGroup = "AppClientId";
    public const string AppRolesGroup = "AppRoles";
    public const string AppPermissionsGroup = "AppPerms";

    private AuthenticationChannel[] channels = [];
    private string[] organisationPersonScopes = [];
    private OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None;
    private string[] personScopes = [];
    private string[] apiKeyScopes = [];
    private string? applicationClientId;
    private string[] applicationRoles = [];
    private string[] applicationPermissions = [];

    public OrganisationAuthorizeAttribute(
        AuthenticationChannel[] channels,
        string[]? organisationPersonScopes = null,
        OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None,
        string[]? personScopes = null,
        string[]? apiKeyScopes = null,
        string? applicationClientId = null,
        string[]? applicationRoles = null,
        string[]? applicationPermissions = null)
    {
        Channels = channels;
        OrganisationPersonScopes = organisationPersonScopes ?? [];
        OrgIdLocation = organisationIdLocation;
        PersonScopes = personScopes ?? [];
        ApiKeyScopes = apiKeyScopes ?? [];
        ApplicationClientId = applicationClientId;
        ApplicationRoles = applicationRoles ?? [];
        ApplicationPermissions = applicationPermissions ?? [];
    }

    public AuthenticationChannel[] Channels
    {
        get => channels;
        set
        {
            channels = value;
            BuildPolicy();
        }
    }

    public string[] OrganisationPersonScopes
    {
        get => organisationPersonScopes;
        set
        {
            organisationPersonScopes = value;
            BuildPolicy();
        }
    }

    public OrganisationIdLocation OrgIdLocation
    {
        get => organisationIdLocation;
        set
        {
            organisationIdLocation = value;
            BuildPolicy();
        }
    }

    public string[] PersonScopes
    {
        get => personScopes;
        set
        {
            personScopes = value;
            BuildPolicy();
        }
    }

    public string[] ApiKeyScopes
    {
        get => apiKeyScopes;
        set
        {
            apiKeyScopes = value;
            BuildPolicy();
        }
    }

    public string? ApplicationClientId
    {
        get => applicationClientId;
        set
        {
            applicationClientId = value;
            BuildPolicy();
        }
    }

    public string[] ApplicationRoles
    {
        get => applicationRoles;
        set
        {
            applicationRoles = value;
            BuildPolicy();
        }
    }

    public string[] ApplicationPermissions
    {
        get => applicationPermissions;
        set
        {
            applicationPermissions = value;
            BuildPolicy();
        }
    }

    private void BuildPolicy()
    {
        Policy = $"{PolicyPrefix}{ChannelsGroup}${string.Join("|", channels)};{OrganisationPersonScopesGroup}${string.Join("|", organisationPersonScopes)};{OrgIdLocGroup}${organisationIdLocation};{PersonScopesGroup}${string.Join("|", personScopes)};{ApiKeyScopesGroup}${string.Join("|", apiKeyScopes)};{AppClientIdGroup}${applicationClientId ?? ""};{AppRolesGroup}${string.Join("|", applicationRoles)};{AppPermissionsGroup}${string.Join("|", applicationPermissions)};";
    }
}
