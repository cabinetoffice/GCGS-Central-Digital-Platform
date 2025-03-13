using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class SupplierToBuyerSelectDevolvedRegulationModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [NotEmpty(ErrorMessageResourceName = nameof(StaticTextResource.SupplierToBuyer_SelectDevolvedRegulation_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public required List<Constants.DevolvedRegulation> Regulations { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    private string SupplierToBuyerStateKey => $"Supplier_To_Buyer_{Id}_Answers";

    public async Task<IActionResult> OnGetAsync()
    {
        var organisationDetails = await organisationClient.GetOrganisationAsync(Id);

        if (organisationDetails.IsBuyer() || organisationDetails.IsPendingBuyer())
        {
            Regulations = organisationDetails.Details.BuyerInformation.DevolvedRegulations.AsDevolvedRegulationList();
        }
        else
        { 
            var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);
            Regulations = state.Regulations;
        }       

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var organisationDetails = await organisationClient.GetOrganisationAsync(Id);

        if (organisationDetails.IsBuyer() || organisationDetails.IsPendingBuyer())
        {
            await organisationClient.UpdateBuyerDevolvedRegulations(Id, Regulations);

            return RedirectToPage("OrganisationOverview", new { Id });
        }
        else
        {
            SupplierToBuyerStateUpdate();

            return RedirectToPage("SupplierToBuyerOrganisationDetailsSummary", new { Id });
        }
    }

    private void SupplierToBuyerStateUpdate()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);
        state.Regulations = Regulations;
        tempDataService.Put(SupplierToBuyerStateKey, state);
    }
}
