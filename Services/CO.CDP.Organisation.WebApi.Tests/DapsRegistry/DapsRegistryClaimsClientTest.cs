using System.Net;
using System.Text;
using CO.CDP.Organisation.WebApi.DapsRegistry;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.DapsRegistry;

public class DapsRegistryClaimsClientTest
{
    private readonly List<HttpRequestMessage> _requests = [];

    private DapsRegistryClaimsClient Client(Func<HttpRequestMessage, HttpResponseMessage> respond, DapsRegistryOptions? options = null)
    {
        var handler = new StubHandler(r => { _requests.Add(r); return respond(r); });
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(DapsRegistryClaimsClient.HttpClientName)).Returns(() => new HttpClient(handler, false));
        return new DapsRegistryClaimsClient(factory.Object, options ?? new DapsRegistryOptions
        {
            BaseUrl = "https://registry.test/application-registry-api/",
            ClientId = "cdp-organisation-api",
            ClientSecret = "secret"
        }, NullLogger<DapsRegistryClaimsClient>.Instance);
    }

    private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static HttpResponseMessage Registry(HttpRequestMessage request) =>
        request.RequestUri!.AbsolutePath.EndsWith("/api/clients/token")
            ? Json("""{"accessToken":"daps-token","tokenType":"Bearer","expiresIn":3600}""")
            : Json("""{"userPrincipalId":"urn:x","organisations":[{"organisationId":"6b06afe8263672a0ceea90aa","organisationName":"Council","organisationSlug":"cdp-x","cdpOrganisationId":"8c1f2d4e-0000-4000-8000-000000000001","applications":[{"applicationId":"6b06afe8263672a0ceea90c8","discriminator":"find-a-tender","slug":"fts","name":"Find a Tender","roles":["Publisher"],"permissions":["can-publish"]}]}]}""");

    [Fact]
    public async Task ItGetsAClientCredentialsTokenThenTheUsersClaims()
    {
        var claims = await Client(Registry).GetClaimsAsync("urn:fdc:gov.uk:2022:abc");

        claims.Should().ContainSingle().Which.CdpOrganisationId.Should().Be("8c1f2d4e-0000-4000-8000-000000000001");
        claims![0].Applications.Should().ContainSingle().Which.Roles.Should().Equal("Publisher");
        _requests.Select(r => r.RequestUri!.ToString()).Should().Equal(
            "https://registry.test/application-registry-api/api/clients/token",
            "https://registry.test/application-registry-api/api/claims/urn%3Afdc%3Agov.uk%3A2022%3Aabc");
        _requests[1].Headers.Authorization!.Parameter.Should().Be("daps-token");
    }

    [Fact]
    public async Task ItReusesTheTokenUntilItNearsExpiry()
    {
        var client = Client(Registry);

        await client.GetClaimsAsync("urn:a");
        await client.GetClaimsAsync("urn:b");

        _requests.Count(r => r.RequestUri!.AbsolutePath.EndsWith("/api/clients/token")).Should().Be(1);
    }

    [Fact]
    public async Task ItReturnsNullWhenTheRegistryFails()
    {
        var claims = await Client(_ => Json("{}", HttpStatusCode.ServiceUnavailable)).GetClaimsAsync("urn:a");

        claims.Should().BeNull();
    }

    [Fact]
    public async Task ItDoesNothingWhenNotConfigured()
    {
        var claims = await Client(Registry, new DapsRegistryOptions()).GetClaimsAsync("urn:a");

        claims.Should().BeNull();
        _requests.Should().BeEmpty();
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
