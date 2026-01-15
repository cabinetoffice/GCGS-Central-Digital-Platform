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
    }

    [Fact]
    public async Task OnGet_WhenOrganisationExists_ReturnsPageResult()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
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
    public async Task OnGet_WhenApiThrowsException_RedirectsToErrorPage()
    {
        var apiException = new OrganisationApiException("API Error", 500, "", null, null);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ThrowsAsync(apiException);

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>();
        ((RedirectToPageResult)result).PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task OnGet_WhenApiThrowsException_LogsError()
    {
        var apiException = new OrganisationApiException("API Error", 500, "", null, null);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ThrowsAsync(apiException);

        await _model.OnGet();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_PopulatesOrganisationDetails()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);

        await _model.OnGet();

        _model.OrganisationDetails.Should().NotBeNull();
        _model.OrganisationDetails.Should().Be(organisation);
    }

    [Fact]
    public async Task BackLinkUrl_ReturnsOrganisationSelectionUrl()
    {
        var id = Guid.NewGuid();
        _model.Id = id;

        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);

        await _model.OnGet();

        _model.BackLinkUrl.Should().Be("/organisation-selection");
    }

    [Fact]
    public async Task OnGet_AlwaysPopulatesTileOne()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);

        await _model.OnGet();

        _model.Tiles.Should().NotBeEmpty();
        _model.Tiles[0].Title.Should().Be(StaticTextResource.OrganisationHome_TileOne_Title);
        _model.Tiles[0].Body.Should().Be(StaticTextResource.OrganisationHome_TileOne_Body);
        _model.Tiles[0].Href.Should().Be($"/organisation/{_model.Id}?origin=organisation-home");
    }

    [Fact]
    public async Task OnGet_WhenSupplier_PopulatesTileSeven()
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
    public async Task OnGet_WhenSupplierWithFvraTool_PopulatesTileEight()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolSupplier)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock
            .Setup(c => c.BuildUrl(ExternalService.FvraTool, "/supplier", _model.Id, null, true,
                It.IsAny<Dictionary<string, string?>>())).Returns("https://fvra-tool.example.com/supplier");

        await _model.OnGet();

        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileEight_Title);
        var tileEight = _model.Tiles.First(tile => tile.Title == StaticTextResource.OrganisationHome_TileEight_Title);
        tileEight.Body.Should().Be(StaticTextResource.OrganisationHome_TileEight_Body);
        tileEight.Href.Should().Be("https://fvra-tool.example.com/supplier");
    }

    [Fact]
    public async Task OnGet_WhenSupplierWithoutFvraTool_DoesNotPopulateTileEight()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolSupplier)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileEight_Title);
    }

    [Fact]
    public async Task OnGet_WhenBuyerWithSearchRegistryPpon_PopulatesTileTwo()
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
    public async Task OnGet_WhenBuyerWithoutSearchRegistryPpon_DoesNotPopulateTileTwo()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileTwo_Title);
    }

    [Fact]
    public async Task OnGet_WhenBuyerWithAiTool_PopulatesTileThree()
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
    public async Task OnGet_WhenBuyerWithoutAiTool_DoesNotPopulateTileThree()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileThree_Title);
    }

    [Fact]
    public async Task OnGet_WhenBuyerWithPayments_PopulatesTileFour()
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
    public async Task OnGet_WhenBuyerWithoutPayments_DoesNotPopulateTileFour()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFour_Title);
    }

    [Fact]
    public async Task OnGet_WhenBuyer_AlwaysPopulatesTileFive()
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
    public async Task OnGet_WhenBuyerWithFvraTool_PopulatesTileSix()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolBuyer)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock
            .Setup(c => c.BuildUrl(ExternalService.FvraTool, "/buyer", _model.Id, null, true,
                It.IsAny<Dictionary<string, string?>>())).Returns("https://fvra-tool.example.com/buyer");

        await _model.OnGet();

        _model.Tiles.Should().Contain(tile => tile.Body == StaticTextResource.OrganisationHome_TileSix_Body);
        var tileSix = _model.Tiles.First(tile => tile.Body == StaticTextResource.OrganisationHome_TileSix_Body);
        tileSix.Body.Should().Be(StaticTextResource.OrganisationHome_TileSix_Body);
        tileSix.Href.Should().Be("https://fvra-tool.example.com/buyer");
    }

    [Fact]
    public async Task OnGet_WhenBuyerWithoutFvraTool_DoesNotPopulateTileSix()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolBuyer)).ReturnsAsync(false);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Body == StaticTextResource.OrganisationHome_TileSix_Body);
    }

    [Fact]
    public async Task OnGet_WhenNotBuyer_DoesNotPopulateBuyerTiles()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolBuyer)).ReturnsAsync(true);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileTwo_Title);
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileThree_Title);
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFour_Title);
        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFive_Title);
        _model.Tiles.Should().NotContain(tile => tile.Body == StaticTextResource.OrganisationHome_TileSix_Body);
    }

    [Fact]
    public async Task OnGet_WhenNotSupplier_DoesNotPopulateSupplierTiles()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolSupplier)).ReturnsAsync(true);

        await _model.OnGet();

        _model.Tiles.Should().NotContain(tile => tile.Title == StaticTextResource.OrganisationHome_TileSeven_Title);
        _model.Tiles.Should().NotContain(tile => tile.Body == StaticTextResource.OrganisationHome_TileEight_Body);
    }

    [Fact]
    public async Task OnGet_WhenCookiesAccepted_PassesTrueToExternalServices()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(
            ExternalService.AiTool, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>()))
            .Returns("https://ai-tool.example.com");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(
            ExternalService.AiTool, "", _model.Id, null, true, It.IsAny<Dictionary<string, string?>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenCookiesRejected_PassesFalseToExternalServices()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Reject);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(
            ExternalService.AiTool, "", _model.Id, null, false, It.IsAny<Dictionary<string, string?>>()))
            .Returns("https://ai-tool.example.com");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(
            ExternalService.AiTool, "", _model.Id, null, false, It.IsAny<Dictionary<string, string?>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenCookiesUnknown_PassesNullToExternalServices()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Unknown);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(
            ExternalService.AiTool, "", _model.Id, null, null, It.IsAny<Dictionary<string, string?>>()))
            .Returns("https://ai-tool.example.com");

        await _model.OnGet();

        _externalServiceUrlBuilderMock.Verify(c => c.BuildUrl(
            ExternalService.AiTool, "", _model.Id, null, null, It.IsAny<Dictionary<string, string?>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGet_PassesOriginParameterToExternalServices()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);

        Dictionary<string, string?>? capturedParams = null;
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(
            It.IsAny<ExternalService>(), It.IsAny<string>(), It.IsAny<Guid?>(),
            It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<Dictionary<string, string?>>()))
            .Callback<ExternalService, string, Guid?, string?, bool?, Dictionary<string, string?>?>(
                (_, _, _, _, _, additionalParams) => capturedParams = additionalParams)
            .Returns("https://example.com");

        await _model.OnGet();

        capturedParams.Should().NotBeNull();
        capturedParams.Should().ContainKey("origin");
        capturedParams!["origin"].Should().Be("organisation-home");
    }

    [Fact]
    public async Task OnGet_WhenBuyerAndSupplier_PopulatesBothBuyerAndSupplierTiles()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer, PartyRole.Tenderer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolBuyer)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolSupplier)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.AiTool)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Payments)).ReturnsAsync(true);
        _featureManagerMock.Setup(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(true);
        _cookiePreferencesServiceMock.Setup(c => c.GetValue()).Returns(CookieAcceptanceValues.Accept);
        _externalServiceUrlBuilderMock.Setup(c => c.BuildUrl(
            It.IsAny<ExternalService>(), It.IsAny<string>(), It.IsAny<Guid?>(),
            It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<Dictionary<string, string?>>()))
            .Returns("https://example.com");
        _ftsUrlServiceMock.Setup(f => f.BuildUrl(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns("https://fts.example.com");

        await _model.OnGet();

        // Should have supplier tiles
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileSeven_Title);
        _model.Tiles.Should().Contain(tile => tile.Body == StaticTextResource.OrganisationHome_TileEight_Body);

        // Should have buyer tiles
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileTwo_Title);
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileThree_Title);
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFour_Title);
        _model.Tiles.Should().Contain(tile => tile.Title == StaticTextResource.OrganisationHome_TileFive_Title);
        _model.Tiles.Should().Contain(tile => tile.Body == StaticTextResource.OrganisationHome_TileSix_Body);
    }

    [Fact]
    public async Task OnGet_VerifiesAllFeatureFlagsAreChecked()
    {
        var organisation = OrganisationFactory.CreateOrganisation(roles: [PartyRole.Buyer]);
        _organisationClientMock.Setup(oc => oc.GetOrganisationAsync(_model.Id)).ReturnsAsync(organisation);

        await _model.OnGet();

        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.SearchRegistryPpon), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.AiTool), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.Payments), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolBuyer), Times.Once);
        _featureManagerMock.Verify(fm => fm.IsEnabledAsync(FeatureFlags.FvraToolSupplier), Times.Once);
    }
}

