using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class SupplierToBuyerDevolvedRegulationModel(ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select 'yes' or 'no'")]
    public bool? Devolved { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    private string SupplierToBuyerStateKey => $"Supplier_To_Buyer_{Id}_Answers";

    public IActionResult OnGet()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);

        Devolved = state.Devolved;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        SupplierToBuyerStateUpdate();

        if (Devolved == true)
        {
            return RedirectToPage("SupplierToBuyerSelectDevolvedRegulation", new { Id });
        }
        else
        {
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
