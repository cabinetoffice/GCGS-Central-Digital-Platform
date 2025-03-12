using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;
public class SupplierToBuyerSelectDevolvedRegulationTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly SupplierToBuyerSelectDevolvedRegulationModel _model;
    private static readonly Guid _orgGuid = Guid.NewGuid();

    public SupplierToBuyerSelectDevolvedRegulationTest()
    {
        _tempDataServiceMock = new Mock<ITempDataService>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierToBuyerSelectDevolvedRegulationModel(_organisationClientMock.Object, _tempDataServiceMock.Object)
        {
            Id = _orgGuid,
            Regulations = new List<Constants.DevolvedRegulation>()
        };
    }

    [Theory]
    [InlineData(new[] { PartyRole.Buyer }, new PartyRole[] { })]  // Organisation is a Buyer
    [InlineData(new PartyRole[] { }, new[] { PartyRole.Buyer })]  // Organisation is a Pending Buyer    
    public async Task OnGetAsync_ShouldSetRegulations_WhenOrganisationIsBuyer(PartyRole[]? partyRoles, PartyRole[]? pendingRoles)
    {
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel(partyRoles, pendingRoles));

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.Regulations.Should().NotBeNull().And.HaveCount(1);
    }

    [Theory]
    [InlineData(new[] { PartyRole.Supplier }, new PartyRole[] { })]
    [InlineData(new PartyRole[] { }, new PartyRole[] { })]
    public async Task OnGetAsync_ShouldPopulateRegulations_FromTempData_WhenOrganisationIsNotBuyer(PartyRole[]? partyRoles, PartyRole[]? pendingRoles)
    {
        var expectedRegulations = new List<Constants.DevolvedRegulation> { Constants.DevolvedRegulation.Wales };
        var tempDataState = new SupplierToBuyerDetails { Regulations = expectedRegulations };

        var organisation = GivenOrganisationClientModel(partyRoles, pendingRoles);
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(organisation);

        _tempDataServiceMock.Setup(x => x.PeekOrDefault<SupplierToBuyerDetails>($"Supplier_To_Buyer_{organisation.Id}_Answers")).Returns(tempDataState);

        _model.Id = organisation.Id;

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.Regulations.Should().BeEquivalentTo(expectedRegulations);
        _tempDataServiceMock.Verify(t => t.PeekOrDefault<SupplierToBuyerDetails>($"Supplier_To_Buyer_{organisation.Id}_Answers"), Times.Once);
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("Regulations", "Required");

        var result = await _model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ShouldSetRegulationsToEmpty_WhenStateIsNotAvailable()
    {
        var organisation = GivenOrganisationClientModel([], []);
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(organisation);

        _tempDataServiceMock
         .Setup(td => td.PeekOrDefault<SupplierToBuyerDetails>(It.IsAny<string>()))
         .Returns(new SupplierToBuyerDetails());

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.Regulations.Should().BeEmpty();
    }

    [Fact]
    public async Task OnPostAsync_ShouldUpdateBuyerRegulations_WhenOrganisationIsBuyer()
    {
        var organisation = GivenOrganisationClientModel([PartyRole.Buyer], []);
        _model.Id = organisation.Id;
        _model.Regulations = new List<Constants.DevolvedRegulation> { Constants.DevolvedRegulation.Scotland };

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(organisation.Id)).ReturnsAsync(organisation);

        var result = await _model.OnPostAsync();

        _organisationClientMock.Verify(o => o.UpdateBuyerInformationAsync(
                                                organisation.Id,
                                                It.Is<UpdateBuyerInformation>(u => u.Type == BuyerInformationUpdateType.DevolvedRegulation))
        , Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }

    [Fact]
    public async Task OnPostAsync_ShouldUpdateTempData_WhenOrganisationIsNotBuyer()
    {
        var organisation = GivenOrganisationClientModel([], [PartyRole.Supplier]);
        var tempDataState = new SupplierToBuyerDetails();

        _model.Id = organisation.Id;
        _model.Regulations = new List<Constants.DevolvedRegulation> { Constants.DevolvedRegulation.Scotland };

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(organisation.Id))
            .ReturnsAsync(organisation);
        _tempDataServiceMock.Setup(x => x.PeekOrDefault<SupplierToBuyerDetails>($"Supplier_To_Buyer_{organisation.Id}_Answers"))
            .Returns(tempDataState);

        var result = await _model.OnPostAsync();

        _tempDataServiceMock.Verify(x => x.Put($"Supplier_To_Buyer_{organisation.Id}_Answers", It.Is<SupplierToBuyerDetails>(s => s.Regulations == _model.Regulations)), Times.Once);
        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerOrganisationDetailsSummary");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(organisation.Id);
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(PartyRole[]? partyRoles = null, PartyRole[]? pendingRoles = null)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(
            additionalIdentifiers: null,
            addresses: null, contactPoint: null,
            id: _orgGuid,
            identifier: null,
            name: "Test Org",
            type: CDP.Organisation.WebApiClient.OrganisationType.Organisation,
            roles: partyRoles,
            details: new Details(
                approval: null,
                buyerInformation: new BuyerInformation(
                    buyerType: "Buyer Type",
                    devolvedRegulations: [CDP.Organisation.WebApiClient.DevolvedRegulation.Wales]),
                pendingRoles: pendingRoles,
                publicServiceMissionOrganization: null,
                scale: null,
                shelteredWorkshop: null,
                vcse: null));
    }
}
