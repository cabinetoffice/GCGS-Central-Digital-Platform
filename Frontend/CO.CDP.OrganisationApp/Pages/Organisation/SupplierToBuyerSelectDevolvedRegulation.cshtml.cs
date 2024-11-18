using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class SupplierToBuyerSelectDevolvedRegulationModel(ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [NotEmpty(ErrorMessage = "Select a devolved region")]
    public required List<DevolvedRegulation> Regulations { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    private string SupplierToBuyerStateKey => $"Supplier_To_Buyer_{Id}_Answers";

    public IActionResult OnGet()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);

        Regulations = state.Regulations;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        SupplierToBuyerStateUpdate();

        return RedirectToPage("SupplierToBuyerOrganisationDetailsSummary", new { Id });
    }

    private void SupplierToBuyerStateUpdate()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);
        state.Regulations = Regulations;
        tempDataService.Put(SupplierToBuyerStateKey, state);
    }
}
