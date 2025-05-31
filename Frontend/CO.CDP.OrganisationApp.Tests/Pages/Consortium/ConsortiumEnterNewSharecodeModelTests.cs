using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit.Sdk;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumEnterNewSharecodeModelTests
{
    private readonly Mock<ITempDataService> _tempDataServiceMock = new();
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private readonly Mock<IDataSharingClient> _dataSharingClientMock = new();
    private static readonly Guid _consortiumId = Guid.NewGuid();
    private readonly ConsortiumEnterNewSharecodeModel _model;
    public ConsortiumEnterNewSharecodeModelTests()
    {
        _model = new ConsortiumEnterNewSharecodeModel(
            _organisationClientMock.Object,
            _dataSharingClientMock.Object,
            _tempDataServiceMock.Object)
        {
            Id = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToPageNotFound_WhenConsortiumIsNull()
    {
        _organisationClientMock
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((CO.CDP.Organisation.WebApiClient.Organisation?)null);

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_ShouldReturnPage_WhenConsortiumExists()
    {
        var organisation = GivenOrganisationClientModel(_consortiumId);
        var organisationParty = GivenOrganisationClientModel(Guid.NewGuid(), "Organisation Party");

        _organisationClientMock
            .Setup(client => client.GetOrganisationAsync(organisation.Id))
            .ReturnsAsync(organisation);

        _organisationClientMock
                    .Setup(client => client.GetOrganisationAsync(organisationParty.Id))
                    .ReturnsAsync(organisationParty);
        _model.Id = organisation.Id;
        _model.PartyId = organisationParty.Id;

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.ConsortiumName.Should().Be(organisation.Name);
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("EnterSharecode", "Required");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPageWithError_WhenPartyIdDoesNotMatchShareCodeId()
    {
        _model.EnterSharecode = "INVALIDCODE";
        _model.PartyId = Guid.NewGuid();

        var shareCode = GetSupplierInfo();

        _dataSharingClientMock.Setup(client => client.GetSharedDataAsync(_model.EnterSharecode))
            .ReturnsAsync(shareCode);

        var result = await _model.OnPost();

        _model.ModelState.IsValid.Should().BeFalse();
        
        result.Should().BeOfType<PageResult>();
        _model.ModelState.First().Value!.Errors.First().ErrorMessage
            .Should().Be(StaticTextResource.Consortium_ConsortiumEnterSharecode_InValidSharecodeError);
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToConsortiumOverview_WhenShareCodeIsValid()
    {
        _model.EnterSharecode = "VALIDCODE";
        _model.Id = Guid.NewGuid();

        var validShareCode = GetSupplierInfo();
        _model.PartyId = validShareCode.Id;

        _dataSharingClientMock.Setup(client => client.GetSharedDataAsync(_model.EnterSharecode))
            .ReturnsAsync(validShareCode);

        _organisationClientMock.Setup(client => client.UpdateOrganisationPartyAsync(
            _model.Id,
            It.IsAny<UpdateOrganisationParty>()
        )).Returns(Task.CompletedTask);

        var redirectResult = await _model.OnPost();

        redirectResult.Should().BeOfType<RedirectToPageResult>();
        redirectResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConsortiumOverview");
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid id, string name = "Test Consortium")
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null, id: id, identifier: null, name: name, type: CDP.Organisation.WebApiClient.OrganisationType.InformalConsortium, roles: [], details: new CO.CDP.Organisation.WebApiClient.Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null));
    }

    private static DataSharing.WebApiClient.SupplierInformation GetSupplierInfo()
    {
        return new DataSharing.WebApiClient.SupplierInformation
        (
            id: Guid.NewGuid(),
            name: "Test Sharecode Organisation",
            associatedPersons: [],
            additionalParties: [],
            additionalEntities: [],
            identifier: null!,
            additionalIdentifiers: [],
            address: new DataSharing.WebApiClient.Address("GB", "UK", "very local", "WS1", "", "1 st", DataSharing.WebApiClient.AddressType.Registered),
            contactPoint: null!,
            roles: [],
            details: null!,
            supplierInformationData: null!,
            type: DataSharing.WebApiClient.OrganisationType.Organisation
        );
    }
}