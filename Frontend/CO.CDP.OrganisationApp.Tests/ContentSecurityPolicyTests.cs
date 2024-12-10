using System.Text.RegularExpressions;
using FluentAssertions;
using CO.CDP.TestKit.Mvc;

namespace CO.CDP.OrganisationApp.Tests;

public class ContentSecurityPolicyTests
{
    private HttpClient BuildHttpClient()
    {
        var factory = new TestWebApplicationFactory<Program>(builder => {});

        return factory.CreateClient();
    }

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
            var actualNonce = match.Groups[1].Value;
            actualNonce.Should().Be(expectedNonce, "The nonce in the script tag should match the one in the CSP header");
        }
    }
}

