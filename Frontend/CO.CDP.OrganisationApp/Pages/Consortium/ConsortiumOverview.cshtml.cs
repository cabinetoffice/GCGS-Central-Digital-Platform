using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
public class ConsortiumOverviewModel() : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("#");
    }
}
