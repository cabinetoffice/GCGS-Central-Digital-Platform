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
 */
public class OrganisationAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Org_";
    public const string ChannelsGroup = "Channels";
    public const string OrganisationPersonScopesGroup = "OrgScopes";
    public const string OrgIdLocGroup = "OrgIdLoc";
    public const string PersonScopesGroup = "PersonScopes";
    public const string ApiKeyScopesGroup = "ApiKeyScopes";

    private AuthenticationChannel[] channels = [];
    private string[] organisationPersonScopes = [];
    private OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None;
    private string[] personScopes = [];
    private string[] apiKeyScopes = [];

    public OrganisationAuthorizeAttribute(
        AuthenticationChannel[] channels,
        string[]? organisationPersonScopes = null,
        OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None,
        string[]? personScopes = null,
        string[]? apiKeyScopes = null)
    {
        Channels = channels;
        OrganisationPersonScopes = organisationPersonScopes ?? [];
        OrgIdLocation = organisationIdLocation;
        PersonScopes = personScopes ?? [];
        ApiKeyScopes = apiKeyScopes ?? [];
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

    private void BuildPolicy()
    {
        Policy = $"{PolicyPrefix}{ChannelsGroup}${string.Join("|", channels)};{OrganisationPersonScopesGroup}${string.Join("|", organisationPersonScopes)};{OrgIdLocGroup}${organisationIdLocation};{PersonScopesGroup}${string.Join("|", personScopes)};{ApiKeyScopesGroup}${string.Join("|", apiKeyScopes)};";
    }
}