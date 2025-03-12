using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;
public class SupplierToBuyerDevolvedRegulationTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly SupplierToBuyerDevolvedRegulationModel _model;
    private static readonly Guid _organisationId = Guid.NewGuid();

    public SupplierToBuyerDevolvedRegulationTest()
    {
        _tempDataServiceMock = new Mock<ITempDataService>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierToBuyerDevolvedRegulationModel(_organisationClientMock.Object, _tempDataServiceMock.Object)
        {
            Id = _organisationId
        };
    }

    [Theory]
    [InlineData(true, new[] { PartyRole.Buyer }, new PartyRole[] { }, new[] { DevolvedRegulation.Scotland} )]
    [InlineData(false, new PartyRole[] { }, new[] { PartyRole.Buyer }, new DevolvedRegulation[] { })]
    public async Task OnGetAsync_ShouldPopulateDevolved_WhenOrganisationIsBuyer(
        bool hasRegulations,
        PartyRole[]? partyRoles,
        PartyRole[]? pendingRoles,
        DevolvedRegulation[]? devolvedRegulations)
    {
        var organisation = GivenOrganisationClientModel(partyRoles, pendingRoles, devolvedRegulations);
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(organisation);
        _model.Id = _organisationId;

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.Devolved.Should().Be(hasRegulations);
    }

    [Fact]
    public async Task OnGetAsync_ShouldPopulateDevolved_FromTempData_WhenOrganisationIsNotBuyer()
    {
        var state = new SupplierToBuyerDetails { Devolved = true };
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(GivenOrganisationClientModel([PartyRole.Supplier], []));

        _tempDataServiceMock.Setup(x => x.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);

        _model.Id = _organisationId;

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.Devolved.Should().BeTrue();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("Devolved", "Required");

        var result = await _model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OnPostAsync_ShouldUpdateState_WhenOrganisationIsNotBuyer(bool devolved)
    {
        _model.Id = _organisationId;
        _model.Devolved = devolved;
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(GivenOrganisationClientModel([PartyRole.Supplier], []));

        var tempDataState = new SupplierToBuyerDetails();
        _tempDataServiceMock.Setup(x => x.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(tempDataState);

        await _model.OnPostAsync();

        _tempDataServiceMock.Verify(x => x.Put(It.IsAny<string>(), It.Is<SupplierToBuyerDetails>(s => s.Devolved == devolved)), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToSelectDevolvedRegulation_WhenDevolvedIsTrue()
    {
        _model.Id = _organisationId;
        _model.Devolved = true;
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(GivenOrganisationClientModel([PartyRole.Buyer], []));

        var result = await _model.OnPostAsync();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerSelectDevolvedRegulation");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(_organisationId);
    }

    [Fact]
    public async Task OnPostAsync_ShouldClearRegulationsAndRedirect_WhenOrganisationIsBuyerAndDevolvedIsFalse()
    {
        _model.Id = _organisationId;
        _model.Devolved = false;
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(GivenOrganisationClientModel([PartyRole.Buyer], []));

        var result = await _model.OnPostAsync();
        _organisationClientMock.Verify(o => o.UpdateBuyerInformationAsync(
                                                _organisationId,
                                                It.Is<UpdateBuyerInformation>(u => u.Type == BuyerInformationUpdateType.DevolvedRegulation))
        , Times.Once);


        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }

    [Fact]
    public async Task OnPostAsync_ShouldRedirectToSummary_WhenDevolvedIsFalseAndOrganisationIsNotBuyer()
    {
        _model.Id = _organisationId;
        _model.Devolved = false;
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_organisationId))
            .ReturnsAsync(GivenOrganisationClientModel([PartyRole.Supplier], []));
        var state = new SupplierToBuyerDetails();
        _tempDataServiceMock
            .Setup(t => t.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
            .Returns(state);
        var result = await _model.OnPostAsync();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerOrganisationDetailsSummary");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(_organisationId);
    }    

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(PartyRole[]? partyRoles = null, PartyRole[]? pendingRoles = null, DevolvedRegulation[]? devolvedRegulations = null)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: null,
            addresses: null, contactPoint: null,
            id: _organisationId,
            identifier: null,
            name: "Test Org",
            type: CDP.Organisation.WebApiClient.OrganisationType.Organisation,
            roles: partyRoles,
            details: new Details(
                approval: null,
                buyerInformation: new BuyerInformation(
                    buyerType: "Buyer Type",
                    devolvedRegulations: devolvedRegulations),
                pendingRoles: pendingRoles,
                publicServiceMissionOrganization: null,
                scale: null,
                shelteredWorkshop: null,
                vcse: null));
    }
}
