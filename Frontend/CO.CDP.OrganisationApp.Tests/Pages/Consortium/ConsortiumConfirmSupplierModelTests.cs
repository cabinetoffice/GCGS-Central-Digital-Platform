using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumConfirmSupplierModelTests
{
    private readonly Mock<ITempDataService> _tempDataServiceMock = new();
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private static readonly Guid _consortiumId = Guid.NewGuid();
    private readonly ConsortiumConfirmSupplierModel _model;
    public ConsortiumConfirmSupplierModelTests()
    {
        _model = new ConsortiumConfirmSupplierModel(
            _organisationClientMock.Object,
            _tempDataServiceMock.Object)
        {
            Id = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task OnGet_ReturnsPage_WhenOrganisationExists()
    {
        var organisation = GivenOrganisationClientModel();
        var sharecode = new ConsortiumSharecode { SharecodeOrganisationName = "Sharecode Org" };

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(organisation);

        _tempDataServiceMock.Setup(x => x.PeekOrDefault<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey))
            .Returns(sharecode);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.ConsortiumName.Should().Be("Test Consortium");
        _model.Heading.Should().Be($"Add {sharecode.SharecodeOrganisationName} to the consortium?");
    }

    [Fact]
    public async Task OnGet_RedirectsToPageNotFound_WhenOrganisationDoesNotExist()
    {
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_model.Id))
            .ReturnsAsync((CO.CDP.Organisation.WebApiClient.Organisation?)null);

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_AddsOrganisationParty_WhenConfirmSupplierIsTrue()
    {
        var organisation = GivenOrganisationClientModel();
        var sharecode = new ConsortiumSharecode
        {
            OrganisationPartyId = Guid.NewGuid(),
            Sharecode = "TestSharecode"
        };

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(organisation);

        _tempDataServiceMock.Setup(x => x.PeekOrDefault<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey))
            .Returns(sharecode);

        _model.ConfirmSupplier = true;

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("ConsortiumOverview");
        _tempDataServiceMock.Verify(x => x.Remove(ConsortiumSharecode.TempDataKey), Times.Never);
        _organisationClientMock.Verify(x => x.AddOrganisationPartyAsync(
            _model.Id,
            It.Is<AddOrganisationParty>(p =>
                p.OrganisationPartyId == sharecode.OrganisationPartyId &&
                p.ShareCode == sharecode.Sharecode &&
                p.OrganisationRelationship == OrganisationRelationship.Consortium
            )
        ), Times.Once);
    }

    [Fact]
    public async Task OnPost_RemovesTempData_WhenConfirmSupplierIsFalse()
    {
        var organisation = GivenOrganisationClientModel();
        var sharecode = new ConsortiumSharecode { SharecodeOrganisationName = "Sharecode Org" };

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_model.Id))
            .ReturnsAsync(organisation);

        _tempDataServiceMock.Setup(x => x.PeekOrDefault<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey))
            .Returns(sharecode);

        _model.ConfirmSupplier = false;

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("ConsortiumOverview");
        _tempDataServiceMock.Verify(x => x.Remove(ConsortiumSharecode.TempDataKey), Times.Once);
    }

    [Fact]
    public async Task OnPost_RedirectsToPageNotFound_WhenOrganisationDoesNotExist()
    {
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_model.Id))
            .ReturnsAsync((CO.CDP.Organisation.WebApiClient.Organisation?)null);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null, id: _consortiumId, identifier: null, name: "Test Consortium", type: CDP.Organisation.WebApiClient.OrganisationType.InformalConsortium, roles: [], details: new CO.CDP.Organisation.WebApiClient.Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null));
    }
}