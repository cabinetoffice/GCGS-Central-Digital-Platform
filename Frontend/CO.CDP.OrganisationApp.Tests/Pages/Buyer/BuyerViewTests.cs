using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Buyer;
using CO.CDP.OrganisationApp.Tests.TestData;
using CO.CDP.UI.Foundation.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using Moq;
using PartyRole = CO.CDP.Organisation.WebApiClient.PartyRole;

namespace CO.CDP.OrganisationApp.Tests.Pages.Buyer;

public class BuyerViewTests
{
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ICommercialToolsUrlService> _commercialToolsUrlServiceMock;
    private readonly Mock<ICookiePreferencesService> _cookiePreferencesServiceMock;
    private readonly BuyerView _model;

    public BuyerViewTests()
    {
        _featureManagerMock = new Mock<IFeatureManager>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _commercialToolsUrlServiceMock = new Mock<ICommercialToolsUrlService>();
        _cookiePreferencesServiceMock = new Mock<ICookiePreferencesService>();
        _model = new BuyerView(
            _featureManagerMock.Object,
            _commercialToolsUrlServiceMock.Object,
            _cookiePreferencesServiceMock.Object)
        {
            Id = Guid.NewGuid()
        };

        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.BuyerView)).ReturnsAsync(true);
    }

    [Fact]
    public async Task OnGet_WhenBuyerViewFeatureIsEnabled_ReturnsPage()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_WhenBuyerViewFeatureIsEnabled_PopulatesTiles()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.CommercialTools)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.IsAccepted()).Returns(true);
        _commercialToolsUrlServiceMock.Setup(c => c.BuildUrl("", _model.Id, null, true)).Returns("https://commercial-tools.example.com");

        await _model.OnGet();

        _model.Tiles.Should().NotBeEmpty();
        _model.Tiles.Should().HaveCount(4);
        _model.Tiles[0].Title.Should().Be(StaticTextResource.BuyerView_TileOne_Title);
        _model.Tiles[0].Body.Should().Be(StaticTextResource.BuyerView_TileOne_Body);
        _model.Tiles[1].Title.Should().Be(StaticTextResource.BuyerView_TileTwo_Title);
        _model.Tiles[1].Body.Should().Be(StaticTextResource.BuyerView_TileTwo_Body);
        _model.Tiles[2].Title.Should().Be(StaticTextResource.BuyerView_TileThree_Title);
        _model.Tiles[2].Body.Should().Be(StaticTextResource.BuyerView_TileThree_Body);
        _model.Tiles[3].Title.Should().Be(StaticTextResource.BuyerView_TileFour_Title);
        _model.Tiles[3].Body.Should().Be(StaticTextResource.BuyerView_TileFour_Body);
    }

    [Fact]
    public async Task OnGet_WhenBuyerViewFeatureIsEnabled_PopulatesTileLinks()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.CommercialTools)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.IsAccepted()).Returns(true);
        _commercialToolsUrlServiceMock.Setup(c => c.BuildUrl("", _model.Id, null, true)).Returns("https://commercial-tools.example.com");

        await _model.OnGet();

        _model.Tiles.Should().NotBeEmpty();
        _model.Tiles[0].Href.Should().Be($"/organisation/{_model.Id}?origin=buyer-view");
        _model.Tiles[1].Href.Should().Be($"/organisation/{_model.Id}/buyer/search?origin=buyer-view");
        _model.Tiles[2].Href.Should().Be("#");
        _model.Tiles[3].Href.Should().Be("https://commercial-tools.example.com");
    }

    [Fact]
    public async Task OnGet_WhenSearchRegistryPponDisabled_TileTwoIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(false);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().ContainSingle(tile => tile.Title == StaticTextResource.BuyerView_TileOne_Title);
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.BuyerView_TileTwo_Title);
    }

    [Fact]
    public async Task OnGet_WhenSearchRegistryPponEnabled_TileTwoIsPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.BuyerView_TileTwo_Title);
    }

    [Fact]
    public async Task OnGet_WhenAiToolDisabled_TileThreeIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(false);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.BuyerView_TileThree_Title);
    }

    [Fact]
    public async Task OnGet_WhenAiToolEnabled_TileThreeIsPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(false);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);

        await _model.OnGet();

        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.BuyerView_TileThree_Title);
    }

    [Fact]
    public async Task OnGet_WhenCommercialToolsDisabled_TileFourIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.CommercialTools)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.BuyerView_TileFour_Title);
    }

    [Fact]
    public async Task OnGet_WhenCommercialToolsEnabled_TileFourIsPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: new List<PartyRole> { PartyRole.Buyer });
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.CommercialTools)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.IsAccepted()).Returns(true);
        _commercialToolsUrlServiceMock.Setup(c => c.BuildUrl("", _model.Id, null, true)).Returns("https://commercial-tools.example.com");

        await _model.OnGet();

        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.BuyerView_TileFour_Title);
    }

    [Fact]
    public async Task OnGet_WhenCommercialToolsEnabled_CallsBuildUrlWithCorrectParameters()
    {
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.CommercialTools)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.IsAccepted()).Returns(true);
        _commercialToolsUrlServiceMock.Setup(c => c.BuildUrl("", _model.Id, null, true)).Returns("https://commercial-tools.example.com");

        await _model.OnGet();

        _commercialToolsUrlServiceMock.Verify(c => c.BuildUrl(
            "",
            _model.Id,
            null,
            true
        ), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenCommercialToolsEnabled_PassesCookiePreferenceCorrectly()
    {
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.CommercialTools)).ReturnsAsync(true);

        _cookiePreferencesServiceMock.Setup(c => c.IsAccepted()).Returns(true);
        _commercialToolsUrlServiceMock.Setup(c => c.BuildUrl("", _model.Id, null, true)).Returns("https://commercial-tools.example.com/with-cookies");

        await _model.OnGet();

        _commercialToolsUrlServiceMock.Verify(c => c.BuildUrl("", _model.Id, null, true), Times.Once);
        _cookiePreferencesServiceMock.Verify(c => c.IsAccepted(), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenCommercialToolsEnabled_PassesCookieRejectionCorrectly()
    {
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.CommercialTools)).ReturnsAsync(true);

        _cookiePreferencesServiceMock.Setup(c => c.IsAccepted()).Returns(false);
        _commercialToolsUrlServiceMock.Setup(c => c.BuildUrl("", _model.Id, null, false)).Returns("https://commercial-tools.example.com/no-cookies");

        await _model.OnGet();

        _commercialToolsUrlServiceMock.Verify(c => c.BuildUrl("", _model.Id, null, false), Times.Once);
        _cookiePreferencesServiceMock.Verify(c => c.IsAccepted(), Times.Once);
    }
}
