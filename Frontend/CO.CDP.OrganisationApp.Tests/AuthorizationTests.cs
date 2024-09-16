using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests;

public class AuthorizationTests
{
    private readonly HttpClient _httpClient;
    private static readonly Mock<TenantClient> tenantClient = new Mock<TenantClient>("https://whatever", new HttpClient());
    private static readonly Mock<PersonClient> personClient = new Mock<PersonClient>("https://whatever", new HttpClient());
    private static readonly Mock<OrganisationClient> organisationClient = new Mock<OrganisationClient>("https://whatever", new HttpClient());
    private static readonly Mock<ISession> _mockSession = new Mock<ISession>();
    private static Guid testOrganisationId = new Guid("0510ce2a-10c9-4c1a-b9cb-3d65c52aa7b7");
    private static Guid personId = new Guid("5b0d3aa8-94cd-4ede-ba03-546937035690");

    public AuthorizationTests()
    {

        var services = new ServiceCollection();

        var organisation = new UserOrganisation(
                                    testOrganisationId,
                                    "Org name",
                                    new List<Tenant.WebApiClient.PartyRole> {
                                        Tenant.WebApiClient.PartyRole.Supplier,
                                        Tenant.WebApiClient.PartyRole.ProcuringEntity
                                    },
                                    new List<string> { "ADMIN" },
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
                    new PersonInviteModel("a@b.com", "First name", new Guid(), "Last name", new List<string>() { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor })
                }
            );

        services.AddTransient<IOrganisationClient, OrganisationClient>(sc => organisationClient.Object);

        _mockSession.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails() { Email = "a@b.com", UserUrn = "urn", PersonId = person.Id });

        services.AddSingleton(_mockSession.Object);

        services.AddSingleton<IPolicyEvaluator, FakeAuthenticationPolicyEvaluator>();

        var factory = new CustomisableWebApplicationFactory<Program>(services);
        _httpClient = factory.CreateClient();
    }

    public static IEnumerable<object[]> SuccessfulTestCases()
    {
        yield return new object[] { $"/organisation/{testOrganisationId}/users/user-summary", "Organisation has 2 users" };
    }

    [Theory]
    [MemberData(nameof(SuccessfulTestCases))]
    public async Task TestAuthorizationIsSuccessful_WhenUserIsAllowedToAccessResource(string url, string expectedText)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.NotNull(responseBody);
        Assert.Contains(expectedText, responseBody);
    }

    public static IEnumerable<object[]> UnsuccessfulTestCases()
    {
        // Note that the organisation ID in these URLs intentionally doesn't match the organisation that is mocked above
        yield return new object[] { $"/organisation/69457513-5cf6-4450-945b-d56e6c195147/users/user-summary" };
    }

    [Theory]
    [MemberData(nameof(UnsuccessfulTestCases))]
    public async Task TestAuthorizationIsUnsuccessful_WhenUserIsNotAllowedToAccessResource(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.NotNull(responseBody);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", responseBody);
    }
}
