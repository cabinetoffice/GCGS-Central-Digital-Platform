using CO.CDP.OrganisationApp.Pages.Organisation;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Moq;
using PartyRole = CO.CDP.Tenant.WebApiClient.PartyRole;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OrganisationSelectionTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<ITenantClient> organisationClientMock;

    public OrganisationSelectionTest()
    {
        sessionMock = new Mock<ISession>();
        sessionMock.Setup(session => session.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails { UserUrn = "urn:test" });
        organisationClientMock = new Mock<ITenantClient>();
    }

    [Fact]
    public async Task OnGet_WhenSessionIsValid_ShouldPopulatePageModel()
    {
        var model = GivenOrganisationSelectionModelModel();

        sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails { UserUrn = "urn:test" });

        organisationClientMock.Setup(o => o.LookupTenantAsync())
            .ReturnsAsync(GetUserTenant());

        var actionResult = await model.OnGet();

        model.UserOrganisations.Should().HaveCount(1);
    }

    private OrganisationSelectionModel GivenOrganisationSelectionModelModel()
    {
        return new OrganisationSelectionModel(organisationClientMock.Object, sessionMock.Object);
    }

    private TenantLookup GetUserTenant()
    {
        return new TenantLookup(
                new List<UserTenant>
                {
                    new(
                        id: Guid.NewGuid(),
                        name: "TrentTheTenant",
                        organisations: new List<UserOrganisation>()
                        {
                            new(
                                id: Guid.NewGuid(),
                                name: "Acme Ltd",
                                roles: new List<PartyRole> { PartyRole.Payee },
                                pendingRoles: [],
                                scopes: new List<string> { "Scope" },
                                uri: new Uri("http://www.acme.com"))
                        }
                    )
                },
                new UserDetails(email: "person@example.com", name: "Person A", scopes: [ "SUPPORTADMIN" ], urn: "urn:test"));
    }
}