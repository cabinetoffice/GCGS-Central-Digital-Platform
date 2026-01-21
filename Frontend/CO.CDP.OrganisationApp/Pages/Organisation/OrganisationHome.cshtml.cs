using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using OrganisationApiException = CO.CDP.Organisation.WebApiClient.ApiException;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(OrgScopeRequirement.Viewer)]
public class OrganisationHomeModel(
    IFeatureManager featureManager,
    IExternalServiceUrlBuilder externalServiceUrlBuilder,
    ICookiePreferencesService cookiePreferencesService,
    IOrganisationClient organisationClient,
    IFtsUrlService ftsUrlService,
    ILogger<OrganisationHomeModel> logger)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public List<Tile> Tiles { get; set; } = [];

    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public string BackLinkUrl { get; private set; } = "/organisation-selection";

    private readonly ILogger<OrganisationHomeModel> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IActionResult> OnGet()
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);

            if (OrganisationDetails == null)
            {
                _logger.LogWarning("Organisation not found for Id: {Id}", Id);
                return Redirect("/page-not-found");
            }
        }
        catch (OrganisationApiException ex)
        {
            var errorMessage =
                $"Error occurred while retrieving organisation details for Id: {Id}";
            var cdpException = new CdpExceptionLogging(errorMessage, "ORG_LOOKUP_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);
            return RedirectToPage("/Error");
        }

        var featureConfig = new FeatureConfiguration(
            SearchRegistryPponEnabled: await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon),
            AiToolEnabled: await featureManager.IsEnabledAsync(FeatureFlags.AiTool),
            PaymentsEnabled: await featureManager.IsEnabledAsync(FeatureFlags.Payments),
            FvraToolBuyerEnabled: await featureManager.IsEnabledAsync(FeatureFlags.FvraToolBuyer),
            FvraToolSupplierEnabled: await featureManager.IsEnabledAsync(FeatureFlags.FvraToolSupplier)
        );

        Tiles = BuildTiles(featureConfig);

        return Page();
    }

    private List<Tile> BuildTiles(FeatureConfiguration featureConfig)
    {
        var tiles = AddCommonTiles();

        if (OrganisationDetails!.IsTenderer())
        {
            AddSupplierTiles(tiles, featureConfig);
        }

        if (OrganisationDetails!.IsBuyer())
        {
            AddBuyerTiles(tiles, featureConfig);
        }

        return tiles;
    }

    private List<Tile> AddCommonTiles()
    {
        return new List<Tile>
        {
            new()
            {
                Title = StaticTextResource.OrganisationHome_TileOne_Title,
                Body = StaticTextResource.OrganisationHome_TileOne_Body,
                Href = $"/organisation/{Id}?origin=organisation-home"
            }
        };
    }

    private void AddSupplierTiles(List<Tile> tiles, FeatureConfiguration featureConfig)
    {
        var originParams = GetOriginParams();
        var cookiesAcceptedValue = GetCookiesAcceptedValue();

        tiles.Add(new Tile
        {
            Title = StaticTextResource.OrganisationHome_TileSeven_Title,
            Body = StaticTextResource.OrganisationHome_TileSeven_Body,
            Href = $"/organisation/{Id}/supplier-information?origin=organisation-home"
        });

        if (featureConfig.FvraToolSupplierEnabled)
        {
            tiles.Add(new Tile
            {
                Title = StaticTextResource.OrganisationHome_TileEight_Title,
                Body = StaticTextResource.OrganisationHome_TileEight_Body,
                Href = externalServiceUrlBuilder.BuildUrl(ExternalService.FvraTool, "/supplier", Id, null, cookiesAcceptedValue, originParams)
            });
        }
    }

    private void AddBuyerTiles(List<Tile> tiles, FeatureConfiguration featureConfig)
    {
        var originParams = GetOriginParams();
        var cookiesAcceptedValue = GetCookiesAcceptedValue();

        if (featureConfig.SearchRegistryPponEnabled)
        {
            tiles.Add(new Tile
            {
                Title = StaticTextResource.OrganisationHome_TileTwo_Title,
                Body = StaticTextResource.OrganisationHome_TileTwo_Body,
                Href = $"/organisation/{Id}/buyer/search?origin=organisation-home"
            });
        }

        if (featureConfig.AiToolEnabled)
        {
            tiles.Add(new Tile
            {
                Title = StaticTextResource.OrganisationHome_TileThree_Title,
                Body = StaticTextResource.OrganisationHome_TileThree_Body,
                Href = externalServiceUrlBuilder.BuildUrl(ExternalService.AiTool, "", Id, null, cookiesAcceptedValue, originParams)
            });
        }

        if (featureConfig.PaymentsEnabled)
        {
            tiles.Add(new Tile
            {
                Title = StaticTextResource.OrganisationHome_TileFour_Title,
                Body = StaticTextResource.OrganisationHome_TileFour_Body,
                Href = externalServiceUrlBuilder.BuildUrl(ExternalService.Payments, "", Id, null, cookiesAcceptedValue, originParams)
            });
        }

        tiles.Add(new Tile
        {
            Title = StaticTextResource.OrganisationHome_TileFive_Title,
            Body = StaticTextResource.OrganisationHome_TileFive_Body,
            Href = ftsUrlService.BuildUrl("/login", Id, "/dashboard")
        });

        if (featureConfig.FvraToolBuyerEnabled)
        {
            tiles.Add(new Tile
            {
                Title = StaticTextResource.OrganisationHome_TileSix_Title,
                Body = StaticTextResource.OrganisationHome_TileSix_Body,
                Href = externalServiceUrlBuilder.BuildUrl(ExternalService.FvraTool, "/buyer", Id, null, cookiesAcceptedValue, originParams)
            });
        }
    }

    private Dictionary<string, string?> GetOriginParams()
    {
        return new Dictionary<string, string?> { { "origin", "organisation-home" } };
    }

    private bool? GetCookiesAcceptedValue()
    {
        var cookiesAccepted = cookiePreferencesService.GetValue();
        return cookiesAccepted switch
        {
            CookieAcceptanceValues.Accept => true,
            CookieAcceptanceValues.Reject => false,
            _ => null
        };
    }

    private sealed record FeatureConfiguration(
        bool SearchRegistryPponEnabled,
        bool AiToolEnabled,
        bool PaymentsEnabled,
        bool FvraToolBuyerEnabled,
        bool FvraToolSupplierEnabled);
}
