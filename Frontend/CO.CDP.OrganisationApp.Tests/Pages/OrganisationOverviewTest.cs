using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OrganisationOverviewTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;    
    private readonly Mock<EntityVerificationClient.IPponClient> _pponClient = new();
    private readonly OrganisationOverviewModel _model;

    public OrganisationOverviewTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new OrganisationOverviewModel(_organisationClientMock.Object, _pponClient.Object);
    }

    [Fact]
    public async Task OnGet_WithValidId_CallsGetOrganisationAsync()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        await _model.OnGet();

        _organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
        _organisationClientMock.Verify(c => c.GetOrganisationReviewsAsync(id), Times.Never);
    }

    [Fact]
    public async Task OnGet_WithPendingBuyers_CallsGetOrganisationReviewsAsync()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id: id, pendingRoles: [PartyRole.Buyer]));

        _organisationClientMock.Setup(o => o.GetOrganisationReviewsAsync(id))
            .ReturnsAsync([new Review(null, null, null, ReviewStatus.Pending)]);

        _organisationClientMock.Setup(o => o.GetOrganisationBuyerInformationAsync(id))
            .ReturnsAsync(new BuyerInformation(buyerType: "RegionalAndLocalGovernment", new List<DevolvedRegulation>()));

        await _model.OnGet();

        _organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
        _organisationClientMock.Verify(c => c.GetOrganisationReviewsAsync(id), Times.Once);
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));
        var id = Guid.NewGuid();
        _model.Id = id;
        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id, ICollection<PartyRole>? pendingRoles = null)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: null,
            addresses: null,
            contactPoint: null,
            id: id ?? Guid.NewGuid(),
            identifier: null,
            name: "Test Org",
            type: OrganisationType.Organisation,
            roles: [],
            details: new Details(
                approval: null,
                pendingRoles: pendingRoles != null ? pendingRoles : []
            ));
    }
}