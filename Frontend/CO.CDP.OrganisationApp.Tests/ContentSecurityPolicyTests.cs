using System.Text.RegularExpressions;
using CO.CDP.TestKit.Mvc;
using UserDetails = CO.CDP.OrganisationApp.Models.UserDetails;
using System.Net;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Match = System.Text.RegularExpressions.Match;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Amazon.SimpleSystemsManagement;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.OrganisationApp.Tests;

public class ContentSecurityPolicyTests
{
    [Fact]
    public async Task CspHeaderShouldBePresentAndMatchNonceInScriptTags()
    {
        var httpClient = BuildHttpClient();

        var response = await httpClient.GetAsync("/");
        response.EnsureSuccessStatusCode();

        var cspHeader = response.Headers.GetValues("Content-Security-Policy").FirstOrDefault();
        cspHeader.Should().NotBeNull("CSP header should be present in the response");

        var noncePattern = @"'nonce-([^']+)'";
        var nonceMatch = Regex.Match(cspHeader!, noncePattern);
        nonceMatch.Success.Should().BeTrue("Nonce should be present in the CSP header");

        var expectedNonce = nonceMatch.Groups[1].Value;

        var responseBody = await response.Content.ReadAsStringAsync();

        var scriptMatches = Regex.Matches(responseBody, @"<script[^>]*nonce=""([^""]+)""");

        scriptMatches.Count.Should().BeGreaterThan(0, "There should be at least one script tag with a nonce attribute");

        foreach (Match match in scriptMatches)
        {
            var actualNonce = WebUtility.HtmlDecode(match.Groups[1].Value);
            actualNonce.Should().Be(expectedNonce, "The nonce in the script tag should match the one in the CSP header");
        }
    }

    private HttpClient BuildHttpClient()
    {
        Mock<ISession> session = new();
        Guid personId = new("5b0d3aa8-94cd-4ede-ba03-546937035690");

        var person = new Person.WebApiClient.Person("a@b.com", "First name", personId, "Last name", null);

        session
            .Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails() { Email = "a@b.com", UserUrn = "urn", PersonId = person.Id });

        var organisationClient = new Mock<IOrganisationClient>();

        organisationClient.Setup(client => client.GetAnnouncementsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Organisation.WebApiClient.Announcement>());

        var factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddSingleton(session.Object);
                services.AddSingleton<IOrganisationClient>(organisationClient.Object);
                services.AddTransient<IAuthenticationSchemeProvider, FakeSchemeProvider>();
                services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options => {
                    options.ClientId = "123";
                    options.Authority = "https://whatever";
                });
                services.RemoveAll<IAmazonSimpleSystemsManagement>();
                services.RemoveAll<IConfigureOptions<KeyManagementOptions>>();
                services.AddDataProtection().DisableAutomaticKeyGeneration();
            });
            builder.ConfigureHostConfiguration(c => c.AddInMemoryCollection(
                [
                    new KeyValuePair<string, string?>("Features:SharedSessions", "false")
                ]
            ));
        });

        return factory.CreateClient();
    }
}
