using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DevolvedRegulation = CO.CDP.OrganisationApp.Constants.DevolvedRegulation;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class OrganisationOverviewModel(IOrganisationClient organisationClient) : PageModel
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public BuyerInformation? BuyerInformation { get; set; }

    public List<DevolvedRegulation>? Regulations { get; set; }
    public Review? Review { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);

            if (OrganisationDetails.IsBuyer() || OrganisationDetails.IsPendingBuyer())
            {
                BuyerInformation = await organisationClient.GetOrganisationBuyerInformationAsync(OrganisationDetails.Id);

                var devolvedRegulations = BuyerInformation.DevolvedRegulations;

                Regulations = devolvedRegulations.AsDevolvedRegulationList();
            }

            if (OrganisationDetails.Details.PendingRoles.Count > 0)
            {
                Review = (await organisationClient.GetOrganisationReviewsAsync(Id)).FirstOrDefault();
            }
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}