using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.Authorization;

/*
 * Examples how the attribute parameters are converted to policy name
 * 
 * [OrganisationAuthorize([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey], ["ADMIN", "VIEWER"], OrganisationIdLocation.Path)]
 * Org_Channels$OneLogin|ServiceKey;Scopes$ADMIN|VIEWER;OrgIdLoc$Path;
 * 
 * [OrganisationAuthorize([AuthenticationChannel.ServiceKey])]
 * Org_Channels$ServiceKey;Scopes$;OrgIdLoc$None;
 */
public class OrganisationAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Org_";
    public const string ChannelsGroup = "Channels";
    public const string ScopesGroup = "Scopes";
    public const string OrgIdLocGroup = "OrgIdLoc";

    private AuthenticationChannel[] channels = [];
    private string[] scopes = [];
    private OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None;

    public OrganisationAuthorizeAttribute(
        AuthenticationChannel[] channels,
        string[]? scopes = null,
        OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None)
    {
        Channels = channels;
        Scopes = scopes ?? [];
        OrgIdLocation = organisationIdLocation;
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

    public string[] Scopes
    {
        get => scopes;
        set
        {
            scopes = value;
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

    private void BuildPolicy()
    {
        Policy = $"{PolicyPrefix}{ChannelsGroup}${string.Join("|", channels)};{ScopesGroup}${string.Join("|", scopes)};{OrgIdLocGroup}${organisationIdLocation};";
    }
}