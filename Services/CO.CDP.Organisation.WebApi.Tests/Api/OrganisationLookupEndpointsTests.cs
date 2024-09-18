using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using System.Security.Claims;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;

public class OrganisationLookupEndpointsTests
{
    [Fact]
    public async Task MyOrganisation_AuthorizationSucceed_WhenChannelIsOrganisationKey()
    {
        var factory = new TestAuthorizationWebApplicationFactory<Program>([new Claim(ClaimType.Channel, Channel.OrganisationKey)]);
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/organisation/me");

        response.StatusCode.Should().NotBe(Forbidden);
    }

    [Fact]
    public async Task MyOrganisation_AuthorizationFailed_WhenChannelIsNotOrganisationKey()
    {
        var factory = new TestAuthorizationWebApplicationFactory<Program>([new Claim(ClaimType.Channel, "unknown")]);
        var httpClient = factory.CreateClient();

        var response = await httpClient.GetAsync("/organisation/me");

        response.StatusCode.Should().Be(Forbidden);
    }
}