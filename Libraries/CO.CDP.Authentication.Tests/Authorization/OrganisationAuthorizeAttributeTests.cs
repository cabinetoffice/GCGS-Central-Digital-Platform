using CO.CDP.Authentication.Authorization;
using FluentAssertions;

namespace CO.CDP.Authentication.Tests.Authorization;
public class OrganisationAuthorizeAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetPolicyCorrectly_WithChannelsAndScopes()
    {
        var channels = new[] { AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey };
        var organisationPersonScopes = new[] { "scope1", "scope2" };
        var organisationIdLocation = OrganisationIdLocation.Path;
        var personScopes = new[] { "scope3", "scope4" };
        var apiKeyScopes = new[] { "apiScope1", "apiScope2" };

        var attribute = new OrganisationAuthorizeAttribute(channels, organisationPersonScopes, organisationIdLocation, personScopes, apiKeyScopes);

        attribute.Policy.Should().Be("Org_Channels$OneLogin|ServiceKey;OrgScopes$scope1|scope2;OrgIdLoc$Path;PersonScopes$scope3|scope4;ApiKeyScopes$apiScope1|apiScope2;AppClientId$;AppRoles$;AppPerms$;");
    }

    [Fact]
    public void Constructor_ShouldSetPolicyCorrectly_WhenScopesIsNull()
    {
        var channels = new[] { AuthenticationChannel.OneLogin };
        var organisationIdLocation = OrganisationIdLocation.None;

        var attribute = new OrganisationAuthorizeAttribute(channels, null, organisationIdLocation);

        attribute.Policy.Should().Be("Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$None;PersonScopes$;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");
    }

    [Fact]
    public void Setting_Channels_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$;OrgIdLoc$None;PersonScopes$;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");

        attribute.Channels = [AuthenticationChannel.OneLogin, AuthenticationChannel.OrganisationKey];

        attribute.Policy.Should().Be("Org_Channels$OneLogin|OrganisationKey;OrgScopes$;OrgIdLoc$None;PersonScopes$;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");
    }

    [Fact]
    public void Setting_OrganisationPersonScopes_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey], ["scope1"]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$scope1;OrgIdLoc$None;PersonScopes$;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");

        attribute.OrganisationPersonScopes = ["scope2", "scope3"];

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$scope2|scope3;OrgIdLoc$None;PersonScopes$;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");
    }

    [Fact]
    public void Setting_OrgIdLocation_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey], ["scope1"]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$scope1;OrgIdLoc$None;PersonScopes$;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");

        attribute.OrgIdLocation = OrganisationIdLocation.Body;

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$scope1;OrgIdLoc$Body;PersonScopes$;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");
    }

    [Fact]
    public void Setting_PersonScopes_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.OneLogin], personScopes: ["scope1"]);

        attribute.Policy.Should().Be("Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$None;PersonScopes$scope1;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");

        attribute.PersonScopes = ["scope2", "scope3"];

        attribute.Policy.Should().Be("Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$None;PersonScopes$scope2|scope3;ApiKeyScopes$;AppClientId$;AppRoles$;AppPerms$;");
    }

    [Fact]
    public void Setting_ApiKeyScopes_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey], apiKeyScopes: ["apiScope1"]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$;OrgIdLoc$None;PersonScopes$;ApiKeyScopes$apiScope1;AppClientId$;AppRoles$;AppPerms$;");

        attribute.ApiKeyScopes = ["apiScope2", "apiScope3"];

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$;OrgIdLoc$None;PersonScopes$;ApiKeyScopes$apiScope2|apiScope3;AppClientId$;AppRoles$;AppPerms$;");
    }

    [Fact]
    public void Constructor_ShouldSetPolicyCorrectly_WithApplicationScope()
    {
        var attribute = new OrganisationAuthorizeAttribute(
            [AuthenticationChannel.OneLogin],
            organisationIdLocation: OrganisationIdLocation.Path,
            applicationClientId: "find-a-tender-client",
            applicationRoles: ["DataManager", "ReportViewer"]);

        attribute.Policy.Should().Be("Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$Path;PersonScopes$;ApiKeyScopes$;AppClientId$find-a-tender-client;AppRoles$DataManager|ReportViewer;AppPerms$;");
    }

    [Fact]
    public void Constructor_ShouldSetPolicyCorrectly_WithApplicationPermissions()
    {
        var attribute = new OrganisationAuthorizeAttribute(
            [AuthenticationChannel.OneLogin],
            organisationIdLocation: OrganisationIdLocation.Path,
            applicationClientId: "daps-client",
            applicationPermissions: ["read:reports", "write:reports"]);

        attribute.Policy.Should().Be("Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$Path;PersonScopes$;ApiKeyScopes$;AppClientId$daps-client;AppRoles$;AppPerms$read:reports|write:reports;");
    }

    [Fact]
    public void Setting_ApplicationClientId_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.OneLogin]);

        attribute.ApplicationClientId = "my-app";

        attribute.Policy.Should().Contain("AppClientId$my-app;");
    }

    [Fact]
    public void Setting_ApplicationRoles_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.OneLogin], applicationClientId: "my-app");

        attribute.ApplicationRoles = ["Admin", "Editor"];

        attribute.Policy.Should().Contain("AppRoles$Admin|Editor;");
    }

    [Fact]
    public void Setting_ApplicationPermissions_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.OneLogin], applicationClientId: "my-app");

        attribute.ApplicationPermissions = ["read:data", "write:data"];

        attribute.Policy.Should().Contain("AppPerms$read:data|write:data;");
    }
}
