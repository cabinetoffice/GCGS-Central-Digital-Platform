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
        if (!await featureManager.IsEnabledAsync(FeatureFlags.BuyerView))
        {
            return RedirectToPage("/PageNotFound");
        }

        var organisation = await organisationClient.GetOrganisationAsync(Id);
        if (organisation == null || !organisation.Roles.Contains(PartyRole.Buyer))
        {
            return RedirectToPage("/PageNotFound");
        }

        Tiles =
        [
            new Tile
            {
                Title = StaticTextResource.BuyerView_TileOne_Title,
                Body = StaticTextResource.BuyerView_TileOne_Body,
                Href = $"/organisation/{Id}"
            },
            new Tile
            {
                Title = StaticTextResource.BuyerView_TileTwo_Title,
                Body = StaticTextResource.BuyerView_TileTwo_Body,
                Href = "/organisation/search"
            },
            new Tile
            {
                Title = StaticTextResource.BuyerView_TileThree_Title,
                Body = StaticTextResource.BuyerView_TileThree_Body,
                Href = "#"
            }
        ];

        return Page();
    }
}