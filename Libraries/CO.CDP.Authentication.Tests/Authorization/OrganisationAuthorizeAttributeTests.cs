using CO.CDP.Authentication.Authorization;
using FluentAssertions;

namespace CO.CDP.Authentication.Tests.Authorization;
public class OrganisationAuthorizeAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetPolicyCorrectly_WithChannelsAndScopes()
    {
        var channels = new[] { AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey };
        var scopes = new[] { "scope1", "scope2" };
        var organisationIdLocation = OrganisationIdLocation.Path;

        var attribute = new OrganisationAuthorizeAttribute(channels, scopes, organisationIdLocation);

        attribute.Policy.Should().Be("Org_Channels$OneLogin|ServiceKey;Scopes$scope1|scope2;OrgIdLoc$Path;");
    }

    [Fact]
    public void Constructor_ShouldSetPolicyCorrectly_WhenScopesIsNull()
    {
        var channels = new[] { AuthenticationChannel.OneLogin };
        var organisationIdLocation = OrganisationIdLocation.None;

        var attribute = new OrganisationAuthorizeAttribute(channels, null, organisationIdLocation);

        attribute.Policy.Should().Be("Org_Channels$OneLogin;Scopes$;OrgIdLoc$None;");
    }

    [Fact]
    public void Setting_Channels_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;Scopes$;OrgIdLoc$None;");

        attribute.Channels = [AuthenticationChannel.OneLogin, AuthenticationChannel.OrganisationKey];

        attribute.Policy.Should().Be("Org_Channels$OneLogin|OrganisationKey;Scopes$;OrgIdLoc$None;");
    }

    [Fact]
    public void Setting_Scopes_ShouldUpdate_olicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey], ["scope1"]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;Scopes$scope1;OrgIdLoc$None;");

        attribute.Scopes = ["scope2", "scope3"];

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;Scopes$scope2|scope3;OrgIdLoc$None;");
    }

    [Fact]
    public void Setting_OrgIdLocation_ShouldUpdatePolicy()
    {
        var attribute = new OrganisationAuthorizeAttribute([AuthenticationChannel.ServiceKey], ["scope1"]);

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;Scopes$scope1;OrgIdLoc$None;");

        attribute.OrgIdLocation = OrganisationIdLocation.Body;

        attribute.Policy.Should().Be("Org_Channels$ServiceKey;Scopes$scope1;OrgIdLoc$Body;");
    }
}