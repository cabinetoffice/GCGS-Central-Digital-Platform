using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]

public class ConsortiumOverviewModel(
    IOrganisationClient organisationClient,
    IFlashMessageService flashMessageService,
    ITempDataService tempDataService) : PageModel
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public OrganisationParties? Parties { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);

            Parties = await organisationClient.GetOrganisationPartiesAsync(Id);

            var sc = tempDataService.Get<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey);

            if (sc != null)
            {
                flashMessageService.SetFlashMessage(
                FlashMessageType.Success,
                heading: string.Format(StaticTextResource.Consortium_ConsortiumOverview_Success_Heading, sc.SharecodeOrganisationName)
                );
            }

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("#");
    }
}
