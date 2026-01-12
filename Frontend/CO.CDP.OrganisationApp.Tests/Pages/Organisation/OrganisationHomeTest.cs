using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Organisation;
using CO.CDP.OrganisationApp.Tests.TestData;
using CO.CDP.UI.Foundation.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using OrganisationApiException = CO.CDP.Organisation.WebApiClient.ApiException;
namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class OrganisationHomeTest
{
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<IExternalServiceUrlBuilder> _externalServiceUrlBuilderMock;
    private readonly Mock<ICookiePreferencesService> _cookiePreferencesServiceMock;
    private readonly Mock<IFtsUrlService> _ftsUrlServiceMock;
    private readonly Mock<ILogger<OrganisationHomeModel>> _loggerMock;
    private readonly OrganisationHomeModel _model;

    public OrganisationHomeTest()
    {
        _featureManagerMock = new Mock<IFeatureManager>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _externalServiceUrlBuilderMock = new Mock<IExternalServiceUrlBuilder>();
        _cookiePreferencesServiceMock = new Mock<ICookiePreferencesService>();
        _ftsUrlServiceMock = new Mock<IFtsUrlService>();
        _loggerMock = new Mock<ILogger<OrganisationHomeModel>>();
        _model = new OrganisationHomeModel(
            _featureManagerMock.Object,
            _externalServiceUrlBuilderMock.Object,
            _cookiePreferencesServiceMock.Object,
            _organisationClientMock.Object,
            _ftsUrlServiceMock.Object,
            _loggerMock.Object)
        {
            Id = Guid.NewGuid()
        };
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.BuyerView)).ReturnsAsync(true);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationExists_ReturnsPage()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        var result = await _model.OnGet();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_WhenOrganisationExists_PopulatesOrganisationDetails()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        await _model.OnGet();
        _model.OrganisationDetails.Should().NotBeNull();
        _model.OrganisationDetails.Should().Be(organisation);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationNotFound_RedirectsToPageNotFound()
    {
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id))
            .ReturnsAsync((CO.CDP.Organisation.WebApiClient.Organisation?)null);
        var result = await _model.OnGet();
        result.Should().BeOfType<RedirectResult>();
        ((RedirectResult)result).Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationNotFound_LogsWarning()
    {
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id))
            .ReturnsAsync((CO.CDP.Organisation.WebApiClient.Organisation?)null);
        await _model.OnGet();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Organisation not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationApiThrowsException_RedirectsToErrorPage()
    {
        var apiException = new OrganisationApiException("API Error", 500, "", null, null);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ThrowsAsync(apiException);
        var result = await _model.OnGet();
        result.Should().BeOfType<RedirectToPageResult>();
        ((RedirectToPageResult)result).PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationApiThrowsException_LogsError()
    {
        var apiException = new OrganisationApiException("API Error", 500, "", null, null);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ThrowsAsync(apiException);
        await _model.OnGet();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Error occurred while retrieving organisation details")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyer_PopulatesTileOne()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        await _model.OnGet();
        _model.Tiles.Should().NotBeEmpty();
        _model.Tiles[0].Title.Should().Be(StaticTextResource.OrganisationHome_TileOne_Title);
        _model.Tiles[0].Body.Should().Be(StaticTextResource.OrganisationHome_TileOne_Body);
        _model.Tiles[0].Href.Should().Be($"/organisation/{_model.Id}?origin=organisation-home");
    }

    [Fact] public async Task OnGet_WhenOrganisationIsSupplier_PopulatesTileOne()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Supplier]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        await _model.OnGet();
        _model.Tiles.Should().NotBeEmpty();
        _model.Tiles[0].Title.Should().Be(StaticTextResource.OrganisationHome_TileOne_Title);
        _model.Tiles[0].Body.Should().Be(StaticTextResource.OrganisationHome_TileOne_Body);
        _model.Tiles[0].Href.Should().Be($"/organisation/{_model.Id}?origin=organisation-home");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsTenderer_PopulatesTileSeven()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        await _model.OnGet();
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileSeven_Title);
        var tileSeven = _model.Tiles.First(tile => tile.Title == StaticTextResource.OrganisationHome_TileSeven_Title);
        tileSeven.Body.Should().Be(StaticTextResource.OrganisationHome_TileSeven_Body);
        tileSeven.Href.Should().Be($"/organisation/{_model.Id}/supplier-information?origin=organisation-home");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsNotTenderer_TileSevenIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileSeven_Title);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyerAndSearchRegistryPponEnabled_PopulatesTileTwo()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(true);
        await _model.OnGet();
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileTwo_Title);
        var tileTwo = _model.Tiles.First(tile => tile.Title == StaticTextResource.OrganisationHome_TileTwo_Title);
        tileTwo.Body.Should().Be(StaticTextResource.OrganisationHome_TileTwo_Body);
        tileTwo.Href.Should().Be($"/organisation/{_model.Id}/buyer/search?origin=organisation-home");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyerButSearchRegistryPponDisabled_TileTwoIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(false);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileTwo_Title);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsNotBuyer_TileTwoIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(true);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileTwo_Title);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyerAndAiToolEnabled_PopulatesTileThree()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock
            .Setup(c => c.BuildUrl(ExternalService.AiTool, "", _model.Id, null, true,
                It.IsAny<Dictionary<string, string?>>())).Returns("https://ai-tool.example.com");
        await _model.OnGet();
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileThree_Title);
        var tileThree = _model.Tiles.First(tile => tile.Title == StaticTextResource.OrganisationHome_TileThree_Title);
        tileThree.Body.Should().Be(StaticTextResource.OrganisationHome_TileThree_Body);
        tileThree.Href.Should().Be("https://ai-tool.example.com");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyerButAiToolDisabled_TileThreeIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(false);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileThree_Title);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsNotBuyer_TileThreeIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileThree_Title);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyerAndPaymentsEnabled_PopulatesTileFour()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock
            .Setup(c => c.BuildUrl(ExternalService.Payments, "", _model.Id, null, true,
                It.IsAny<Dictionary<string, string?>>())).Returns("https://payments.example.com");
        await _model.OnGet();
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFour_Title);
        var tileFour = _model.Tiles.First(tile => tile.Title == StaticTextResource.OrganisationHome_TileFour_Title);
        tileFour.Body.Should().Be(StaticTextResource.OrganisationHome_TileFour_Body);
        tileFour.Href.Should().Be("https://payments.example.com");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyerButPaymentsDisabled_TileFourIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(false);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFour_Title);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsNotBuyer_TileFourIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(true);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFour_Title);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyer_PopulatesTileFive()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _ftsUrlServiceMock.Setup(f => f.BuildUrl("/login", _model.Id, "/dashboard"))
            .Returns("https://fts.example.com/login");
        await _model.OnGet();
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFive_Title);
        var tileFive = _model.Tiles.First(tile => tile.Title == StaticTextResource.OrganisationHome_TileFive_Title);
        tileFive.Body.Should().Be(StaticTextResource.OrganisationHome_TileFive_Body);
        tileFive.Href.Should().Be("https://fts.example.com/login");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsNotBuyer_TileFiveIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFive_Title);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsBuyerAndFvraToolEnabled_PopulatesTileSix()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock
            .Setup(c => c.BuildUrl(ExternalService.FvraTool, "", _model.Id, null, true,
                It.IsAny<Dictionary<string, string?>>())).Returns("https://fvra-tool.example.com");

        await _model.OnGet();
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileSix_Title);
        var tileSix = _model.Tiles.First(tile => tile.Title == StaticTextResource.OrganisationHome_TileSix_Title);
        tileSix.Body.Should().Be(StaticTextResource.OrganisationHome_TileSix_Body);
        tileSix.Href.Should().Be("https://fvra-tool.example.com");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationIsSupplierAndFvraToolEnabled_PopulatesTileSix()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Supplier]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock
            .Setup(c => c.BuildUrl(ExternalService.FvraTool, "", _model.Id, null, true,
                It.IsAny<Dictionary<string, string?>>())).Returns("https://fvra-tool.example.com");

        await _model.OnGet();
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileSix_Title);
        var tileSix = _model.Tiles.First(tile => tile.Title == StaticTextResource.OrganisationHome_TileSix_Title);
        tileSix.Body.Should().Be(StaticTextResource.OrganisationHome_TileSix_Body);
        tileSix.Href.Should().Be("https://fvra-tool.example.com");
    }

    [Fact]
    public async Task OnGet_WhenFvraToolDisabled_TileSixIsNotPresent()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraTool)).ReturnsAsync(false);
        await _model.OnGet();
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileSix_Title);
    }

    [Fact]
    public async Task OnGet_WhenAiToolEnabled_CallsBuildUrlWithCorrectParameters()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.AiTool, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>())).Returns("https://ai-tool.example.com");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(
            ExternalService.AiTool,
            "",
            _model.Id,
            null,
            true,
            It.IsAny<Dictionary<string, string?>>()
        ), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenAiToolEnabled_PassesCookiePreferenceCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.AiTool, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>())).Returns("https://ai-tool.example.com/with-cookies");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(ExternalService.AiTool, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>()), Times.Once);
        _cookiePreferencesServiceMock.Verify(c => c.GetValue(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task OnGet_WhenAiToolEnabled_PassesCookieRejectionCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Reject);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.AiTool, "", _model.Id, null, false, It.IsAny<Dictionary<string, string?>>())).Returns("https://ai-tool.example.com/no-cookies");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(ExternalService.AiTool, "", _model.Id, null, false, It.IsAny<Dictionary<string, string?>>()), Times.Once);
        _cookiePreferencesServiceMock.Verify(c => c.GetValue(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task OnGet_WhenAiToolEnabled_PassesOriginParameterCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);

        Dictionary<string, string?>? capturedParams = null;
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(It.IsAny<ExternalService>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<Dictionary<string, string?>>()))
            .Callback<ExternalService, string, Guid?, string?, bool?, Dictionary<string, string?>?>((service, _, _, _, _, additionalParams) =>
            {
                if (service == ExternalService.AiTool)
                    capturedParams = additionalParams;
            })
            .Returns("https://ai-tool.example.com");

        await _model.OnGet();

        capturedParams.Should().NotBeNull();
        capturedParams.Should().ContainKey("origin");
        capturedParams!["origin"].Should().Be("organisation-home");
    }

    [Fact]
    public async Task OnGet_WhenPaymentsEnabled_CallsBuildUrlWithCorrectParameters()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.Payments, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>())).Returns("https://payments.example.com");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(
            ExternalService.Payments,
            "",
            _model.Id,
            null,
            true,
            It.IsAny<Dictionary<string, string?>>()
        ), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenPaymentsEnabled_PassesCookiePreferenceCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.Payments, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>())).Returns("https://payments.example.com/with-cookies");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(ExternalService.Payments, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>()), Times.Once);
        _cookiePreferencesServiceMock.Verify(c => c.GetValue(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task OnGet_WhenPaymentsEnabled_PassesCookieRejectionCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Reject);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.Payments, "", _model.Id, null, false, It.IsAny<Dictionary<string, string?>>())).Returns("https://payments.example.com/no-cookies");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(ExternalService.Payments, "", _model.Id, null, false, It.IsAny<Dictionary<string, string?>>()), Times.Once);
        _cookiePreferencesServiceMock.Verify(c => c.GetValue(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task OnGet_WhenPaymentsEnabled_PassesOriginParameterCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);

        Dictionary<string, string?>? capturedParams = null;
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(It.IsAny<ExternalService>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<Dictionary<string, string?>>()))
            .Callback<ExternalService, string, Guid?, string?, bool?, Dictionary<string, string?>?>((service, _, _, _, _, additionalParams) =>
            {
                if (service == ExternalService.Payments)
                    capturedParams = additionalParams;
            })
            .Returns("https://payments.example.com");

        await _model.OnGet();

        capturedParams.Should().NotBeNull();
        capturedParams.Should().ContainKey("origin");
        capturedParams!["origin"].Should().Be("organisation-home");
    }

    [Fact]
    public async Task OnGet_WhenFvraToolEnabled_CallsBuildUrlWithCorrectParameters()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.FvraTool, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>())).Returns("https://fvra-tool.example.com");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(
            ExternalService.FvraTool,
            "",
            _model.Id,
            null,
            true,
            It.IsAny<Dictionary<string, string?>>()
        ), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenFvraToolEnabled_PassesCookiePreferenceCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.FvraTool, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>())).Returns("https://fvra-tool.example.com/with-cookies");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(ExternalService.FvraTool, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>()), Times.Once);
        _cookiePreferencesServiceMock.Verify(c => c.GetValue(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task OnGet_WhenFvraToolEnabled_PassesCookieRejectionCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Reject);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(ExternalService.FvraTool, "", _model.Id, null, false, It.IsAny<Dictionary<string, string?>>())).Returns("https://fvra-tool.example.com/no-cookies");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(ExternalService.FvraTool, "", _model.Id, null, false, It.IsAny<Dictionary<string, string?>>()), Times.Once);
        _cookiePreferencesServiceMock.Verify(c => c.GetValue(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task OnGet_WhenFvraToolEnabled_PassesOriginParameterCorrectly()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);

        Dictionary<string, string?>? capturedParams = null;
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(It.IsAny<ExternalService>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<Dictionary<string, string?>>()))
            .Callback<ExternalService, string, Guid?, string?, bool?, Dictionary<string, string?>?>((service, _, _, _, _, additionalParams) =>
            {
                if (service == ExternalService.FvraTool)
                    capturedParams = additionalParams;
            })
            .Returns("https://fvra-tool.example.com");

        await _model.OnGet();

        capturedParams.Should().NotBeNull();
        capturedParams.Should().ContainKey("origin");
        capturedParams!["origin"].Should().Be("organisation-home");
    }
}


