using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Buyer;

public class BuyerView(
    IFeatureManager featureManager,
    IOrganisationClient organisationClient)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public List<Tile> Tiles { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        var buyerViewEnabled = await featureManager.IsEnabledAsync(FeatureFlags.BuyerView);
        var searchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);
        var aiToolEnabled = await featureManager.IsEnabledAsync(FeatureFlags.AiTool);

        if (!buyerViewEnabled)
        {
            return RedirectToPage("/PageNotFound");
        }

        var organisation = await organisationClient.GetOrganisationAsync(Id);
        if (organisation == null || !organisation.Roles.Contains(PartyRole.Buyer))
        {
            return RedirectToPage("/PageNotFound");
        }

        var tiles = new List<Tile>
        {
            new()
            {
                Title = StaticTextResource.BuyerView_TileOne_Title,
                Body = StaticTextResource.BuyerView_TileOne_Body,
                Href = $"/organisation/{Id}"
            }
        };

        if (searchRegistryPponEnabled)
        {
            tiles.Add(new Tile
            {
                Title = StaticTextResource.BuyerView_TileTwo_Title,
                Body = StaticTextResource.BuyerView_TileTwo_Body,
                Href = $"/organisation/{Id}/buyer/search"
            });
        }


        if (aiToolEnabled)
        {
            tiles.Add(new Tile
            {
                Title = StaticTextResource.BuyerView_TileThree_Title,
                Body = StaticTextResource.BuyerView_TileThree_Body,
                Href = "#"
            });
        }

        Tiles = tiles;
        return Page();
    }
}