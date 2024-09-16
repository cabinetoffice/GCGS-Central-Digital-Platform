using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization.Policy;
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

    public AuthorizationTests()
    {
    }

    public HttpClient BuildHttpClient(List<string> userScopes)
    {
        var services = new ServiceCollection();

        var organisation = new UserOrganisation(
                                    testOrganisationId,
                                    "Org name",
                                    new List<Tenant.WebApiClient.PartyRole> {
                                        Tenant.WebApiClient.PartyRole.Supplier,
                                        Tenant.WebApiClient.PartyRole.ProcuringEntity
                                    },
                                    userScopes,
                                    new Uri("http://foo")
                                );

        var person = new Person.WebApiClient.Person("a@b.com", "First name", personId, "Last name");

        tenantClient.Setup(client => client.LookupTenantAsync())
            .ReturnsAsync(
                new TenantLookup(
                    new List<UserTenant>() {
                        new UserTenant(
                            new Guid(),
                            "Tenant name",
                            new List<UserOrganisation> { organisation }
                        )
                    },
                    new UserDetails("a@b.com", "User name", "urn")
                ));

        services.AddTransient<ITenantClient, TenantClient>(sc => tenantClient.Object);


        organisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(
                new List<Organisation.WebApiClient.Person>
                {
                    new Organisation.WebApiClient.Person("a@b.com", "First name", person.Id, "Last name", new List<string>() { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor })
                }
            );


        organisationClient.Setup(client => client.GetOrganisationPersonInvitesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(
                new List<PersonInviteModel>
                {
                    new PersonInviteModel("a@b.com", "Person invite", personInviteGuid, "Last name", new List<string>() { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor })
                }
            );

        organisationClient.Setup(client => client.GetOrganisationAsync(testOrganisationId))
            .ReturnsAsync(
                new Organisation.WebApiClient.Organisation(
                    new List<Identifier> { },
                    new List<Address> { },
                    new ContactPoint("a@b.com", "Contact", "123", new Uri("http://whatever")),
                    testOrganisationId,
                    new Identifier("asd", "asd", "asd", new Uri("http://whatever")),
                    "Org name",
                    new List<Organisation.WebApiClient.PartyRole> { Organisation.WebApiClient.PartyRole.Supplier, Organisation.WebApiClient.PartyRole.ProcuringEntity }
                )
            );

        services.AddTransient<IOrganisationClient, OrganisationClient>(sc => organisationClient.Object);

        _mockSession.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails() { Email = "a@b.com", UserUrn = "urn", PersonId = person.Id });

        services.AddSingleton(_mockSession.Object);

        services.AddSingleton<IPolicyEvaluator, FakeAuthenticationPolicyEvaluator>();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddScheme<AuthenticationSchemeOptions, FakeCookieAuthHandler>(CookieAuthenticationDefaults.AuthenticationScheme, options => { });

        var factory = new CustomisableWebApplicationFactory<Program>(services);
        return factory.CreateClient();
    }

    public static IEnumerable<object[]> TestCases()
    {
        yield return new object[] { $"/organisation/{testOrganisationId}/users/user-summary", new string[] { "Organisation has 2 users" } };
        yield return new object[] { $"/organisation/{testOrganisationId}/users/{personInviteGuid}/change-role?handler=personInvite", new string[] { "Person invite Last name", "Can add, remove and edit users" } };
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TestAuthorizationIsSuccessful_WhenUserIsAllowedToAccessResourceAsAdminUser(string url, string[] expectedTexts)
    {
        var _httpClient = BuildHttpClient(new List<string> { OrganisationPersonScopes.Admin });

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.NotNull(responseBody);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        foreach (string expectedText in expectedTexts)
        {
            Assert.Contains(expectedText, responseBody);
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TestAuthorizationIsUnsuccessful_WhenUserIsNotAllowedToAccessResourceAsEditorUser(string url, string[] _)
    {
        var _httpClient = BuildHttpClient(new List<string> { OrganisationPersonScopes.Editor });

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.NotNull(responseBody);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", responseBody);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task TestAuthorizationIsUnsuccessful_WhenUserIsNotAllowedToAccessResourceAsUserWithoutPermissions(string url, string[] _)
    {
        var _httpClient = BuildHttpClient(new List<string> { });

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.NotNull(responseBody);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", responseBody);
    }

    [Fact]
    public async Task TestCanSeeUsersLinkOnOrganisationPage_WhenUserIsAllowedToAccessResourceAsAdminUser()
    {
        var _httpClient = BuildHttpClient(new List<string> { OrganisationPersonScopes.Admin });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/organisation/{testOrganisationId}");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.NotNull(responseBody);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Organisation details", responseBody);
        Assert.Contains($"href=\"/organisation/{testOrganisationId}/users/user-summary\">Users</a>", responseBody);
    }

    [Fact]
    public async Task TestCannotSeeUsersLinkOnOrganisationPage_WhenUserIsNotAllowedToAccessResourceAsEditorUser()
    {
        var _httpClient = BuildHttpClient(new List<string> { OrganisationPersonScopes.Editor });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/organisation/{testOrganisationId}");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.NotNull(responseBody);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.Contains("Organisation details", responseBody);
        Assert.DoesNotContain($"href=\"/organisation/{testOrganisationId}/users/user-summary\">Users</a>", responseBody);
        Assert.DoesNotContain("Users", responseBody);
    }
}
