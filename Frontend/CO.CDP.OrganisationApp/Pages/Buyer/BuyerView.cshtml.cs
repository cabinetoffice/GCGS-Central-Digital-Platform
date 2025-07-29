using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Buyer;
[FeatureGate(FeatureFlags.BuyerView)]
[Authorize(PolicyNames.PartyRole.Buyer)]
[Authorize(OrgScopeRequirement.Viewer)]
public class BuyerView(
    IFeatureManager featureManager)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public List<Tile> Tiles { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        var searchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);
        var aiToolEnabled = await featureManager.IsEnabledAsync(FeatureFlags.AiTool);

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