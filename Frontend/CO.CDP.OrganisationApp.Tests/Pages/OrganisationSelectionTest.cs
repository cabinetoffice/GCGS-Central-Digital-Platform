using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Moq;
using PartyRole = CO.CDP.Tenant.WebApiClient.PartyRole;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OrganisationSelectionTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<ITenantClient> tenantClientMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;

    public OrganisationSelectionTest()
    {
        sessionMock = new Mock<ISession>();
        sessionMock.Setup(session => session.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails { UserUrn = "urn:test" });
        tenantClientMock = new Mock<ITenantClient>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Fact]
    public async Task OnGet_WhenSessionIsValid_ShouldPopulatePageModel()
    {
        var model = GivenOrganisationSelectionModelModel();

        sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails { UserUrn = "urn:test" });

        tenantClientMock.Setup(o => o.LookupTenantAsync())
            .ReturnsAsync(GetUserTenant());

        var actionResult = await model.OnGet();

        model.UserOrganisations.Should().HaveCount(1);
    }

    [Fact]
    public async Task OnGet_WhenOrgHasPendingRoles_ShouldCallGetOrganisationReviewsAsync()
    {
        var model = GivenOrganisationSelectionModelModel();

        sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails { UserUrn = "urn:test" });

        tenantClientMock.Setup(o => o.LookupTenantAsync())
            .ReturnsAsync(GetUserTenant(pendingRoles: [PartyRole.Buyer]));

        organisationClientMock.Setup(o => o.GetOrganisationReviewsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([GivenReview(ReviewStatus.Pending)]);

        var actionResult = await model.OnGet();

        organisationClientMock.Verify(c => c.GetOrganisationReviewsAsync(It.IsAny<Guid>()), Times.Once);

        model.UserOrganisations.Should().HaveCount(1);
        model.UserOrganisations?.FirstOrDefault().Review?.Status.Should().Be(ReviewStatus.Pending);
    }

    [Fact]
    public async Task OnGet_WhenOrgHasPendingRoles_AndIsRejected_CallToGetOrganisationReviewsAsync_ShouldReturnRejectedStatus()
    {
        var model = GivenOrganisationSelectionModelModel();

        sessionMock.Setup(s => s.Get<Models.UserDetails>(Session.UserDetailsKey))
            .Returns(new Models.UserDetails { UserUrn = "urn:test" });

        tenantClientMock.Setup(o => o.LookupTenantAsync())
            .ReturnsAsync(GetUserTenant(pendingRoles: [PartyRole.Buyer]));

        organisationClientMock.Setup(o => o.GetOrganisationReviewsAsync(It.IsAny<Guid>()))
            .ReturnsAsync([GivenReview(ReviewStatus.Rejected)]);

        var actionResult = await model.OnGet();

        organisationClientMock.Verify(c => c.GetOrganisationReviewsAsync(It.IsAny<Guid>()), Times.Once);

        model.UserOrganisations.Should().HaveCount(1);
        model.UserOrganisations?.FirstOrDefault().Review?.Status.Should().Be(ReviewStatus.Rejected);
    }

    private OrganisationSelectionModel GivenOrganisationSelectionModelModel()
    {
        return new OrganisationSelectionModel(tenantClientMock.Object, organisationClientMock.Object, sessionMock.Object);
    }

    private static Review GivenReview(ReviewStatus reviewStatus)
    {
        return new Review(null, null, null, reviewStatus);
    }

    private TenantLookup GetUserTenant(ICollection<PartyRole>? pendingRoles = null)
    {
        return new TenantLookup(
                new List<UserTenant>
                {
                    new(
                        id: new Guid(),
                        name: "TrentTheTenant",
                        organisations: new List<UserOrganisation>()
                        {
                            new(
                                id: Guid.NewGuid(),
                                name: "Acme Ltd",
                                roles: new List<PartyRole> { PartyRole.Payee },
                                pendingRoles: pendingRoles != null ? pendingRoles : [],
                                scopes: new List<string> { "Scope" },
                                uri: new Uri("http://www.acme.com"))
                        }
                    )
                },
                new UserDetails("person@example.com", "Person A", "urn:test"));
    }
}