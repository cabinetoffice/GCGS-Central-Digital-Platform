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

    [Fact]
    public async Task OnGet_WithBuyerSignedMou_SetsMouSignedOnDate()
    {
        var organisationId = Guid.NewGuid();

        var person = new CO.CDP.Organisation.WebApiClient.Person(
            id: Guid.NewGuid(),
            firstName: "John",
            lastName: "Doe",
            email: "john.doe@example.com",
            scopes: new List<string> { "Scope1" }
        );

        var mou = new CO.CDP.Organisation.WebApiClient.Mou(
            id: Guid.NewGuid(),
            filePath: @"\mou-pdfs\mou-pdf-template.pdf",
            createdOn: new DateTimeOffset(2025, 1, 14, 0, 0, 0, TimeSpan.Zero)
        );

        var mouSignatureLatest = new MouSignatureLatest(
            createdBy: person,
            id: Guid.NewGuid(),
            isLatest: true,
            jobTitle: "Manager",
            mou: mou,
            name: $"{person.FirstName} {person.LastName}",
            signatureOn: new DateTimeOffset(2025, 1, 14, 0, 0, 0, TimeSpan.Zero)
        );

        _model.Id = organisationId;

        _organisationClientMock.Setup(o => o.GetOrganisationAsync(organisationId))
            .ReturnsAsync(GivenOrganisationClientModel(organisationId, new List<PartyRole> { PartyRole.Buyer }));

        _organisationClientMock.Setup(o => o.GetOrganisationBuyerInformationAsync(organisationId))
            .ReturnsAsync(new BuyerInformation("RegionalAndLocalGovernment", new List<DevolvedRegulation>()));

        _organisationClientMock.Setup(o => o.GetOrganisationLatestMouSignatureAsync(organisationId))
            .ReturnsAsync(mouSignatureLatest);

        _organisationClientMock.Setup(o => o.GetLatestMouAsync())
            .ReturnsAsync(mou);

        await _model.OnGet();

        _model.HasBuyerSignedMou.Should().BeTrue();

        _model.MouSignedOnDate.Should().Be($"Agreed on 14 January 2025");
    }

    [Fact]
    public async Task OnGet_WithNoBuyerSignedMou_SetsHasBuyerSignedMouToFalse()
    {
        var id = Guid.NewGuid();

        _model.Id = id;

        _organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id, new List<PartyRole> { PartyRole.Buyer }));

        _organisationClientMock.Setup(o => o.GetOrganisationBuyerInformationAsync(id))
            .ReturnsAsync(new BuyerInformation("RegionalAndLocalGovernment", new List<DevolvedRegulation>()));

        _organisationClientMock.Setup(o => o.GetOrganisationReviewsAsync(id))
            .ReturnsAsync(new List<Review> { new Review(null, null, null, ReviewStatus.Pending) });

        _organisationClientMock.Setup(o => o.GetOrganisationLatestMouSignatureAsync(id))
            .ThrowsAsync(new ApiException("Not found", 404, "", null, null));

        await _model.OnGet();

        _model.HasBuyerSignedMou.Should().BeFalse();
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
            , null, null, null, null), buyerInformation: null);
    }
}