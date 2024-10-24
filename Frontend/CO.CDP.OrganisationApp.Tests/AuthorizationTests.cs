using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using CO.CDP.TestKit.Mvc;
using Microsoft.Extensions.Hosting;

namespace CO.CDP.OrganisationApp.Tests;
public class AuthorizationTests
{
    private static readonly Mock<TenantClient> TenantClient = new("https://whatever", new HttpClient());
    private static readonly Mock<PersonClient> PersonClient = new("https://whatever", new HttpClient());
    private static readonly Mock<OrganisationClient> OrganisationClient = new("https://whatever", new HttpClient());
    private static readonly Mock<ISession> Session = new();
    private static readonly Guid OrganisationId = new("0510ce2a-10c9-4c1a-b9cb-3d65c52aa7b7");
    private static readonly Guid PersonId = new("5b0d3aa8-94cd-4ede-ba03-546937035690");
    private static readonly Guid PersonInviteGuid = new("330fb1d4-26e2-4c69-898f-6197f9321361");

    private HttpClient BuildHttpClient(List<string> userOrganisationScopes, List<string> userScopes)
    {
        var organisation = new UserOrganisation(
                                    id: OrganisationId,
                                    name: "Org name",
                                    roles:
                                    [
                                        Tenant.WebApiClient.PartyRole.Supplier,
                                        Tenant.WebApiClient.PartyRole.Tenderer
                                    ],
                                    pendingRoles: [],
                                    scopes: userOrganisationScopes,
                                    uri: new Uri("https://example.com")
                                );

        var person = new Person.WebApiClient.Person("a@b.com", "First name", PersonId, "Last name", userScopes);

        TenantClient.Setup(client => client.LookupTenantAsync())
            .ReturnsAsync(
                new TenantLookup(
                    [
                        new UserTenant(
                            new Guid(),
                            "Tenant name",
                            [ organisation ]
                        )
                    ],
                    new UserDetails("a@b.com", "User name", "urn")
                ));

        OrganisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(
                [
                    new CO.CDP.Organisation.WebApiClient.Person("a@b.com", "First name", person.Id, "Last name", [ OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor ])
                ]
            );


        OrganisationClient.Setup(client => client.GetOrganisationPersonInvitesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(
                [
                    new PersonInviteModel("a@b.com", "Person invite", PersonInviteGuid, "Last name", [ OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor ])
                ]
            );

        OrganisationClient.Setup(client => client.GetOrganisationAsync(OrganisationId))
            .ReturnsAsync(
                new CO.CDP.Organisation.WebApiClient.Organisation(
                    additionalIdentifiers: [],
                    addresses: [],
                    contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
                    id: OrganisationId,
                    identifier: new Identifier("asd", "asd", "asd", new Uri("http://whatever")),
                    name: "Org name",
                    roles: [ Organisation.WebApiClient.PartyRole.Supplier, Organisation.WebApiClient.PartyRole.Tenderer ],
                    details: new Details(approval: null, pendingRoles: [])
                )
            );

        PersonClient.Setup(client => client.LookupPersonAsync(It.IsAny<string>())).ReturnsAsync(person);

        Session.Setup(s => s.Get<Models.UserDetails>(OrganisationApp.Session.UserDetailsKey))
            .Returns(new Models.UserDetails() { Email = "a@b.com", UserUrn = "urn", PersonId = person.Id });

        var factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddTransient<ITenantClient, TenantClient>(_ => TenantClient.Object);
                services.AddTransient<IOrganisationClient, OrganisationClient>(_ => OrganisationClient.Object);
                services.AddTransient<IPersonClient, PersonClient>(_ => PersonClient.Object);
                services.AddSingleton(Session.Object);
                services.AddTransient<IAuthenticationSchemeProvider, FakeSchemeProvider>();
                services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options => {
                    options.ClientId = "123";
                    options.Authority = "https://whatever";
                });
            });
        });

        return factory.CreateClient();
    }

    public static IEnumerable<object[]> TestCases()
    {
        yield return new object[] { $"/organisation/{OrganisationId}/users/user-summary", new[] { "Organisation has 2 users" } };
        yield return new object[] { $"/organisation/{OrganisationId}/users/{PersonInviteGuid}/change-role?handler=personInvite", new[] { "Person invite Last name", "Select a role" } };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TestAuthorizationIsSuccessful_WhenUserIsAllowedToAccessResourceAsAdminUser(string url, string[] expectedTexts)
    {
        var httpClient = BuildHttpClient([OrganisationPersonScopes.Admin], []);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (string expectedText in expectedTexts)
        {
            responseBody.Should().Contain(expectedText);
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TestAuthorizationIsUnsuccessful_WhenUserIsNotAllowedToAccessResourceAsEditorUser(string url, string[] _)
    {
        var httpClient = BuildHttpClient([OrganisationPersonScopes.Editor], []);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseBody.Should().Contain("Page not found");
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TestAuthorizationIsUnsuccessful_WhenUserIsNotAllowedToAccessResourceAsUserWithoutPermissions(string url, string[] _)
    {
        using var httpClient = BuildHttpClient([], []);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseBody.Should().Contain("Page not found");
    }

    [Fact]
    public async Task TestCanSeeUsersLinkOnOrganisationPage_WhenUserIsAllowedToAccessResourceAsAdminUser()
    {
        var httpClient = BuildHttpClient([ OrganisationPersonScopes.Admin, OrganisationPersonScopes.Viewer ], []);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/organisation/{OrganisationId}");

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().Contain("Organisation details");
        responseBody.Should().Contain($"href=\"/organisation/{OrganisationId}/users/user-summary\">Users</a>");
    }

    [Fact]
    public async Task TestCannotSeeUsersLinkOnOrganisationPage_WhenUserIsNotAllowedToAccessResourceAsEditorUser()
    {
        var httpClient = BuildHttpClient([ OrganisationPersonScopes.Editor ], []);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/organisation/{OrganisationId}");

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("Organisation details");
        responseBody.Should().NotContain($"href=\"/organisation/{OrganisationId}/users/user-summary\">Users</a>");
        responseBody.Should().NotContain("Users");
    }

    public static IEnumerable<object[]> SupportAdminAccessTestCases()
    {
        yield return new object[] { $"/organisation/{OrganisationId}", new[] { "Organisation name", "Organisation identifier", "Organisation email", "Supplier information" }, new[] { "Change", "Users" }, HttpStatusCode.OK };
        yield return new object[] { $"/organisation/{OrganisationId}/address/uk?frm-overview", new string[] {}, new string[] {}, HttpStatusCode.NotFound };
        yield return new object[] { $"/organisation/{OrganisationId}/users/user-summary", new string[] {}, new string[] {}, HttpStatusCode.NotFound };
    }

    [Theory]
    [MemberData(nameof(SupportAdminAccessTestCases))]
    public async Task TestAuthorizationIsSuccessful_WhenUserIsAllowedToAccessResourceAsSupportAdminUser(string url, string[] shouldContain, string[] shouldNotContain, HttpStatusCode expectedStatusCode)
    {
        var httpClient = BuildHttpClient([], [PersonScopes.SupportAdmin]);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(expectedStatusCode);
        foreach (string expectedText in shouldContain)
        {
            responseBody.Should().Contain(expectedText);
        }

        foreach (string expectedText in shouldNotContain)
        {
            responseBody.Should().NotContain(expectedText);
        }
    }
}
