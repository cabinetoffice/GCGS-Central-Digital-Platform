using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
[ValidateConsortiumStep]
public class ConsortiumOverviewModel(ISession session) : ConsortiumStepModel(session)
{
    public override string CurrentPage => ConsortiumOverviewPage;

    public IActionResult OnGet()
    {
        return Page();
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("#");
    }
}
