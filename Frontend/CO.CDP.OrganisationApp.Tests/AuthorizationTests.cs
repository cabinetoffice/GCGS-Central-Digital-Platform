using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests;
public class AuthorizationTests
{
    private static readonly Mock<TenantClient> tenantClient = new Mock<TenantClient>("https://whatever", new HttpClient());
    private static readonly Mock<PersonClient> personClient = new Mock<PersonClient>("https://whatever", new HttpClient());
    private static readonly Mock<OrganisationClient> organisationClient = new Mock<OrganisationClient>("https://whatever", new HttpClient());
    private static readonly Mock<ISession> _mockSession = new Mock<ISession>();
    private static Guid testOrganisationId = new Guid("0510ce2a-10c9-4c1a-b9cb-3d65c52aa7b7");
    private static Guid personId = new Guid("5b0d3aa8-94cd-4ede-ba03-546937035690");
    private static Guid personInviteGuid = new Guid("330fb1d4-26e2-4c69-898f-6197f9321361");

    public HttpClient BuildHttpClient(List<string> userOrganisationScopes, List<string> userScopes)
    {
        var services = new ServiceCollection();

        var organisation = new UserOrganisation(
                                    id: testOrganisationId,
                                    name: "Org name",
                                    roles:
                                    [
                                        Tenant.WebApiClient.PartyRole.Supplier,
                                        Tenant.WebApiClient.PartyRole.Tenderer
                                    ],
                                    pendingRoles: [],
                                    scopes: userOrganisationScopes,
                                    uri: new Uri("http://foo")
                                );

        var person = new Person.WebApiClient.Person("a@b.com", "First name", personId, "Last name", userScopes);

        tenantClient.Setup(client => client.LookupTenantAsync())
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

        services.AddTransient<ITenantClient, TenantClient>(sc => tenantClient.Object);


        organisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(
                [
                    new CO.CDP.Organisation.WebApiClient.Person("a@b.com", "First name", person.Id, "Last name", [ OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor ])
                ]
            );


        organisationClient.Setup(client => client.GetOrganisationPersonInvitesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(
                [
                    new PersonInviteModel("a@b.com", "Person invite", personInviteGuid, "Last name", [ OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor ])
                ]
            );

        organisationClient.Setup(client => client.GetOrganisationAsync(testOrganisationId))
            .ReturnsAsync(
                new CO.CDP.Organisation.WebApiClient.Organisation(
                    additionalIdentifiers: [],
                    addresses: [],
                    contactPoint: new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
                    id: testOrganisationId,
                    identifier: new Identifier("asd", "asd", "asd", new Uri("http://whatever")),
                    name: "Org name",
                    roles: [ Organisation.WebApiClient.PartyRole.Supplier, Organisation.WebApiClient.PartyRole.Tenderer ],
                    details: new Details(approval: null, pendingRoles: [])
                )
            );

        personClient.Setup(client => client.LookupPersonAsync(It.IsAny<string>())).ReturnsAsync(person);

        services.AddTransient<IOrganisationClient, OrganisationClient>(sc => organisationClient.Object);

        services.AddTransient<IPersonClient, PersonClient>(sc => personClient.Object);

        _mockSession.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails() { Email = "a@b.com", UserUrn = "urn", PersonId = person.Id });

        services.AddSingleton(_mockSession.Object);

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddScheme<AuthenticationSchemeOptions, FakeCookieAuthHandler>(CookieAuthenticationDefaults.AuthenticationScheme, options => { });

        services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options => {
            options.ClientId = "123";
            options.Authority = "https://whatever";
        });

        var factory = new CustomisableWebApplicationFactory<Program>(services);

        return factory.CreateClient();
    }

    public static IEnumerable<object[]> TestCases()
    {
        yield return new object[] { $"/organisation/{testOrganisationId}/users/user-summary", new string[] { "Organisation has 2 users" } };
        yield return new object[] { $"/organisation/{testOrganisationId}/users/{personInviteGuid}/change-role?handler=personInvite", new string[] { "Person invite Last name", "Select a role" } };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TestAuthorizationIsSuccessful_WhenUserIsAllowedToAccessResourceAsAdminUser(string url, string[] expectedTexts)
    {
        var _httpClient = BuildHttpClient([OrganisationPersonScopes.Admin], []);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

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
        var _httpClient = BuildHttpClient([OrganisationPersonScopes.Editor], []);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseBody.Should().Contain("Page not found");
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TestAuthorizationIsUnsuccessful_WhenUserIsNotAllowedToAccessResourceAsUserWithoutPermissions(string url, string[] _)
    {
        var _httpClient = BuildHttpClient([], []);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseBody.Should().Contain("Page not found");
    }

    [Fact]
    public async Task TestCanSeeUsersLinkOnOrganisationPage_WhenUserIsAllowedToAccessResourceAsAdminUser()
    {
        var _httpClient = BuildHttpClient([ OrganisationPersonScopes.Admin, OrganisationPersonScopes.Viewer ], []);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/organisation/{testOrganisationId}");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().Contain("Organisation details");
        responseBody.Should().Contain($"href=\"/organisation/{testOrganisationId}/users/user-summary\">Users</a>");
    }

    [Fact]
    public async Task TestCannotSeeUsersLinkOnOrganisationPage_WhenUserIsNotAllowedToAccessResourceAsEditorUser()
    {
        var _httpClient = BuildHttpClient([ OrganisationPersonScopes.Editor ], []);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/organisation/{testOrganisationId}");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        responseBody.Should().Contain("Organisation details");
        responseBody.Should().NotContain($"href=\"/organisation/{testOrganisationId}/users/user-summary\">Users</a>");
        responseBody.Should().NotContain("Users");
    }

    public static IEnumerable<object[]> SupportAdminAccessTestCases()
    {
        yield return new object[] { $"/organisation/{testOrganisationId}", new string[] { "Organisation name", "Organisation identifier", "Organisation email", "Supplier information" }, new string[] { "Change", "Users" }, HttpStatusCode.OK };
        yield return new object[] { $"/organisation/{testOrganisationId}/address/uk?frm-overview", new string[] {}, new string[] {}, HttpStatusCode.NotFound };
        yield return new object[] { $"/organisation/{testOrganisationId}/users/user-summary", new string[] {}, new string[] {}, HttpStatusCode.NotFound };
    }

    [Theory]
    [MemberData(nameof(SupportAdminAccessTestCases))]
    public async Task TestAuthorizationIsSuccessful_WhenUserIsAllowedToAccessResourceAsSupportAdminUser(string url, string[] shouldContain, string[] shouldNotContain, HttpStatusCode expectedStatusCode)
    {
        var _httpClient = BuildHttpClient([], [PersonScopes.SupportAdmin]);

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

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
