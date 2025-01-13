using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumEnterSharecodeModelTests
{
    private readonly Mock<ITempDataService> _tempDataServiceMock = new();
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private readonly Mock<IDataSharingClient> _dataSharingClientMock = new();
    private static readonly Guid _consortiumId = Guid.NewGuid();
    private readonly ConsortiumEnterSharecodeModel _model;
    public ConsortiumEnterSharecodeModelTests()
    {
        _model = new ConsortiumEnterSharecodeModel(
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
        var organisation = GivenOrganisationClientModel();

        _organisationClientMock
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(organisation);

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
    public async Task OnPost_ShouldReturnPage_WhenSharecodeAlreadyExists()
    {
        var p = new OrganisationParty(Guid.NewGuid(), "Consortium 1", new OrganisationPartyShareCode(DateTimeOffset.Now, "EXISTING_CODE"));
        var parties = new OrganisationParties([]);
        parties.Parties.Add(p);

        _dataSharingClientMock
            .Setup(client => client.GetSharedDataAsync(It.IsAny<string>()))
            .ReturnsAsync(GetSupplierInfo());

        _organisationClientMock
            .Setup(client => client.GetOrganisationPartiesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(parties);

        _model.EnterSharecode = "EXISTING_CODE";

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ModelState.Should().ContainSingle(m => m.Key == nameof(_model.EnterSharecode));
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToConfirmSupplier_WhenSharecodeIsValid()
    {
        var parties = new OrganisationParties([]);

        _dataSharingClientMock
            .Setup(client => client.GetSharedDataAsync(It.IsAny<string>()))
            .ReturnsAsync(GetSupplierInfo());

        _organisationClientMock
            .Setup(client => client.GetOrganisationPartiesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(parties);

        _model.EnterSharecode = "VALID_CODE";

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConsortiumConfirmSupplier");
        _tempDataServiceMock.Verify(service => service.Put(ConsortiumSharecode.TempDataKey,
            It.Is<ConsortiumSharecode>(sc => sc.Sharecode == "VALID_CODE" && sc.SharecodeOrganisationName == "Test Sharecode Organisation")));
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null, id: _consortiumId, identifier: null, name: "Test Consortium", type: CDP.Organisation.WebApiClient.OrganisationType.InformalConsortium, roles: [], details: new CO.CDP.Organisation.WebApiClient.Details(approval: null, pendingRoles: []));
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
            supplierInformationData: null!
        );
    }
}