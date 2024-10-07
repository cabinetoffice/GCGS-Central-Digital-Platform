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

        var attribute = new OrganisationAuthorizeAttribute(channels, organisationPersonScopes, organisationIdLocation, personScopes);

        attribute.Policy.Should().Be("Org_Channels$OneLogin|ServiceKey;OrgScopes$scope1|scope2;OrgIdLoc$Path;PersonScopes$scope3|scope4;");
    }

    [Fact]
    public void Constructor_ShouldSetPolicyCorrectly_WhenScopesIsNull()
    {
        var channels = new[] { AuthenticationChannel.OneLogin };
        var organisationIdLocation = OrganisationIdLocation.None;

        var attribute = new OrganisationAuthorizeAttribute(channels, null, organisationIdLocation);

        attribute.Policy.Should().Be("Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$None;PersonScopes$;");
    }

    [Fact]
    public void Setting_Channels_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$;OrgIdLoc$None;PersonScopes$;");

        attribute.Channels = [AuthenticationChannel.OneLogin, AuthenticationChannel.OrganisationKey];

        attribute.Policy.Should().Be("Org_Channels$OneLogin|OrganisationKey;OrgScopes$;OrgIdLoc$None;PersonScopes$;");
    }

    [Fact]
    public void Setting_OrganisationPersonScopes_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey], ["scope1"]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$scope1;OrgIdLoc$None;PersonScopes$;");

        attribute.OrganisationPersonScopes = ["scope2", "scope3"];

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$scope2|scope3;OrgIdLoc$None;PersonScopes$;");
    }

    [Fact]
    public void Setting_OrgIdLocation_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey], ["scope1"]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$scope1;OrgIdLoc$None;PersonScopes$;");

        attribute.OrgIdLocation = OrganisationIdLocation.Body;

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;OrgScopes$scope1;OrgIdLoc$Body;PersonScopes$;");
    }

    [Fact]
    public void Setting_PersonScopes_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.OneLogin], personScopes: ["scope1"]);

        attribute.Policy.Should().Be("Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$None;PersonScopes$scope1;");

        attribute.PersonScopes = ["scope2", "scope3"];

        attribute.Policy.Should().Be("Org_Channels$OneLogin;OrgScopes$;OrgIdLoc$None;PersonScopes$scope2|scope3;");
    }
}