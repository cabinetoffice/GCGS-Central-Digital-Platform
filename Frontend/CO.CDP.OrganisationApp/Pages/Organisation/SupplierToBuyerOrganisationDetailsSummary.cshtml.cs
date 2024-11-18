using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class SupplierToBuyerOrganisationDetailsSummaryModel(
    ITempDataService tempDataService,
    OrganisationWebApiClient.IOrganisationClient organisationClient) : PageModel
{
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public SupplierToBuyerDetails? SupplierToBuyerDetailsModel { get; set; }
    private string SupplierToBuyerStateKey => $"Supplier_To_Buyer_{Id}_Answers";
    public IActionResult OnGet()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);

        if (state.BuyerOrganisationType == null)
        {
            return RedirectToPage("SupplierToBuyerOrganisationType", new { Id });
        }

        SupplierToBuyerDetailsModel = new SupplierToBuyerDetails
        {
            OrganisationType = state.OrganisationType,
            BuyerOrganisationOtherValue = state.BuyerOrganisationOtherValue,
            BuyerOrganisationType = state.BuyerOrganisationType,
            Devolved = state.Devolved,
            Regulations = state.Devolved == true ? state.Regulations : [],
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (state.BuyerOrganisationType != null)
        {
            await organisationClient.UpdateOrganisationAddAsBuyerRole(Id, state);

            tempDataService.Remove(SupplierToBuyerStateKey);
            tempDataService.Put(FlashMessageTypes.Success, new FlashMessage(
                  "Buyer registration pending approval"));

            return RedirectToPage("OrganisationOverview", new { Id });
        }

        return RedirectToPage("SupplierToBuyerOrganisationType", new { Id });
    }
}
