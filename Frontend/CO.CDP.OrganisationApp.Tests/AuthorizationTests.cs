using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using CO.CDP.TestKit.Mvc;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;

public class AuthorizationTests
{
    private readonly HttpClient _httpClient;
    private static readonly Mock<TenantClient> tenantClient = new Mock<TenantClient>("https://whatever", new HttpClient());
    private static readonly Mock<PersonClient> personClient = new Mock<PersonClient>("https://whatever", new HttpClient());
    private static readonly Mock<OrganisationClient> organisationClient = new Mock<OrganisationClient>("https://whatever", new HttpClient());
    private static readonly Mock<CO.CDP.OrganisationApp.ISession> _mockSession = new Mock<CO.CDP.OrganisationApp.ISession>();
    private Guid testOrganisationId = new Guid("0510ce2a-10c9-4c1a-b9cb-3d65c52aa7b7");
    private Guid personId = new Guid("5b0d3aa8-94cd-4ede-ba03-546937035690");

    public AuthorizationTests()
    {
        
        var services = new ServiceCollection();

        var organisation = new UserOrganisation(
                                    testOrganisationId,
                                    "Org name",
                                    new List<CO.CDP.Tenant.WebApiClient.PartyRole> {
                                        CO.CDP.Tenant.WebApiClient.PartyRole.Supplier,
                                        CO.CDP.Tenant.WebApiClient.PartyRole.ProcuringEntity
                                    },
                                    new List<string> { "ADMIN" },
                                    new Uri("http://foo")
                                );

        var person = new CO.CDP.Person.WebApiClient.Person("a@b.com", "First name", personId, "Last name");

        personClient.Setup(client => client.LookupPersonAsync(It.IsAny<string>()))
            .ReturnsAsync(person);


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
                new List<CO.CDP.Organisation.WebApiClient.Person>
                {
                    new CO.CDP.Organisation.WebApiClient.Person("a@b.com", "First name", person.Id, "Last name", new List<string>() { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor })
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

        _mockSession.Setup(s => s.Get<CO.CDP.OrganisationApp.Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new CO.CDP.OrganisationApp.Models.UserDetails() { Email = "a@b.com", UserUrn = "urn", PersonId = person.Id });

        services.AddSingleton<CO.CDP.OrganisationApp.ISession>(_mockSession.Object);
        
        // TODO: Ask why this is needed
        services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

        var factory = new TestCustomWebApplicationFactory<Program>(services);
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task TempTest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/organisation/{testOrganisationId}/users/user-summary");

        var response = await _httpClient.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.NotNull(responseBody);
        Assert.Contains("blah", responseBody);
    }
}

public class TestCustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly IServiceCollection _replacementServices;

    public TestCustomWebApplicationFactory(IServiceCollection replacementServices)
    {
        _replacementServices = replacementServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(async services =>
        {
            if (_replacementServices != null)
            {
                foreach (var replacementService in _replacementServices)
                {
                    var serviceDescriptorForServiceToReplace = services.FirstOrDefault(descriptor => descriptor.ServiceType == replacementService.ServiceType);
                    if (serviceDescriptorForServiceToReplace != null)
                    {
                        services.Remove(serviceDescriptorForServiceToReplace);
                        services.Add(replacementService);
                    }
                }
            }
        });
    }
}
