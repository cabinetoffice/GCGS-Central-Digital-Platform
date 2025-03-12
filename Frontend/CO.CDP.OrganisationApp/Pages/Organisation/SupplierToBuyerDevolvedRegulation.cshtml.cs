using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class SupplierToBuyerDevolvedRegulationModel(IOrganisationClient organisationClient, ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.SupplierToBuyer_DevolvedRegulation_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? Devolved { get; set; }

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
            Devolved = organisationDetails.Details.BuyerInformation.DevolvedRegulations.Any();
        }
        else
        {
            var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);

            Devolved = state.Devolved;
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

        if (!(organisationDetails.IsBuyer() || organisationDetails.IsPendingBuyer()))
        {
            SupplierToBuyerStateUpdate();
        }

        if (Devolved == true)
        {
            return RedirectToPage("SupplierToBuyerSelectDevolvedRegulation", new { Id });
        }
        else
        {
            if (organisationDetails.IsBuyer() || organisationDetails.IsPendingBuyer())
            {
                await organisationClient.UpdateBuyerDevolvedRegulations(Id, []);

                return RedirectToPage("OrganisationOverview", new { Id });
            }

            return RedirectToPage("SupplierToBuyerOrganisationDetailsSummary", new { Id });
        }
    }

    private void SupplierToBuyerStateUpdate()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);
        state.Devolved = Devolved;
        tempDataService.Put(SupplierToBuyerStateKey, state);
    }
}
