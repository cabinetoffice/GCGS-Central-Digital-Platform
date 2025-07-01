using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;

namespace CO.CDP.OrganisationApp.Pages.Buyer;

public class BuyerView(IFeatureManager featureManager, IOrganisationClient organisationClient) : PageModel
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
                Title = "View your organisation details",
                Body = "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.",
                Href = "#"
            },
            new Tile
            {
                Title = "Public procurement organisation number (PPON) register",
                Body = "Quisque blandit tellus ac sapien rutrum vehicula praesent vel gravida felis.",
                Href = "#"
            },
            new Tile
            {
                Title = "Statement of work assurance tool",
                Body = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium.",
                Href = "#"
            }
        ];

        return Page();
    }
}