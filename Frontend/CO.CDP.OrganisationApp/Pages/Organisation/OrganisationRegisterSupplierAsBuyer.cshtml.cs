using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Admin)] // TODO **** : ASK before PR the question to Luke <authorize scope="@OrgScopeRequirement.Admin">
public class OrganisationRegisterSupplierAsBuyerModel(
   ITempDataService tempDataService,
   IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    private string SupplierToBuyerStateKey => $"Supplier_To_Buyer_{Id}_Answers";

    public async Task<IActionResult> OnGet(Guid id)
    {
        var orgInfo = await organisationClient.GetOrganisationAsync(id);
        if (!orgInfo.Roles.Contains(PartyRole.Buyer)) // add isPendding logic check here if possible - also can you add multiple buyers
        {
            var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);
            state.OrganisationType = orgInfo.IsTenderer() ? OrganisationType.Supplier : OrganisationType.Buyer; // check this logic

            tempDataService.Put(SupplierToBuyerStateKey, state);

            return RedirectToPage("SupplierToBuyerOrganisationType", new { id });
        }

        return RedirectToPage("OrganisationOverview", new { id });
    }


}
