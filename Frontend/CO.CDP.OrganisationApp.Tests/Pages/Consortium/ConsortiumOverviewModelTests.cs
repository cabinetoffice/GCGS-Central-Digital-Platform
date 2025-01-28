using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumOverviewModelTests
{
    private readonly Mock<ITempDataService> _tempDataServiceMock = new();
    private readonly Mock<IFlashMessageService> _flashMessageServiceMock = new();
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private static readonly Guid _consortiumId = Guid.NewGuid();
    private readonly ConsortiumOverviewModel _pageModel;
    public ConsortiumOverviewModelTests()
    {
        _pageModel = new ConsortiumOverviewModel(
            _organisationClientMock.Object,
            _flashMessageServiceMock.Object,
            _tempDataServiceMock.Object
        )
        {
            Id = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task OnGet_ReturnsPage_WhenOrganisationExists()
    {
        var organisation = GivenOrganisationClientModel();
        var parties = new OrganisationParties(new List<OrganisationParty>
        {
            new OrganisationParty(Guid.NewGuid(), "Consortium 1", new OrganisationPartyShareCode(DateTimeOffset.Now, "EXISTING_CODE"))
        });

        var sharecode = new ConsortiumSharecode { SharecodeOrganisationName = "Sharecode Org" };

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_pageModel.Id)).ReturnsAsync(organisation);
        _organisationClientMock.Setup(x => x.GetOrganisationPartiesAsync(_pageModel.Id)).ReturnsAsync(parties);
        _tempDataServiceMock.Setup(x => x.Get<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey)).Returns(sharecode);

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _pageModel.OrganisationDetails.Should().BeEquivalentTo(organisation);
        _pageModel.Parties.Should().BeEquivalentTo(parties);
        _flashMessageServiceMock.Verify(x => x.SetFlashMessage
            (FlashMessageType.Success,
            string.Format(StaticTextResource.Consortium_ConsortiumOverview_Success_Heading, sharecode.SharecodeOrganisationName),
            null, null, null, null), Times.Once);
    }

    [Fact]
    public async Task OnGet_ReturnsPage_WhenPartiesNotFound()
    {
        var organisation = GivenOrganisationClientModel();

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_pageModel.Id))
            .ReturnsAsync(organisation);

        _organisationClientMock.Setup(x => x.GetOrganisationPartiesAsync(_pageModel.Id))
            .ThrowsAsync(new ApiException("Not Found", 404, null, null, null));

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _pageModel.OrganisationDetails.Should().BeEquivalentTo(organisation);
        _pageModel.Parties.Should().BeNull();
    }

    [Fact]
    public async Task OnGet_RedirectsToPageNotFound_WhenOrganisationNotFound()
    {
        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_pageModel.Id))
            .ThrowsAsync(new ApiException("Not Found", 404, null, null, null));

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_SetsFlashMessage_WhenSharecodeExists()
    {
        var organisation = GivenOrganisationClientModel();
        var parties = new OrganisationParties(new List<OrganisationParty>
        {
            new OrganisationParty(Guid.NewGuid(), "Consortium 1", new OrganisationPartyShareCode(DateTimeOffset.Now, "EXISTING_CODE"))
        });
        var sharecode = new ConsortiumSharecode { SharecodeOrganisationName = "Sharecode Org" };

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(_pageModel.Id))
            .ReturnsAsync(organisation);

        _organisationClientMock.Setup(x => x.GetOrganisationPartiesAsync(_pageModel.Id))
            .ReturnsAsync(parties);

        _tempDataServiceMock.Setup(x => x.Get<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey)).Returns(sharecode);

        var result = await _pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        _flashMessageServiceMock.Verify(x => x.SetFlashMessage(
            FlashMessageType.Success,
            string.Format(StaticTextResource.Consortium_ConsortiumOverview_Success_Heading, sharecode.SharecodeOrganisationName),
            null, null, null, null), Times.Once);
    }
    
    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null, id: _consortiumId, identifier: null, name: "Test Consortium", type: CDP.Organisation.WebApiClient.OrganisationType.InformalConsortium, roles: [], details: new CO.CDP.Organisation.WebApiClient.Details(approval: null, pendingRoles: [], null, null, null, null), buyerInformation: null);
    }
}