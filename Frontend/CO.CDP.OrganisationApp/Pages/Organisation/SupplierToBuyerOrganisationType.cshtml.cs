using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class SupplierToBuyerOrganisationTypeModel(ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.SupplierToBuyer_OrganisationType_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? BuyerOrganisationType { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.SupplierToBuyer_OrganisationType_EnterType))]
    [RequiredIf(nameof(StaticTextResource.SupplierToBuyer_OrganisationType_BuyerType), nameof(StaticTextResource.SupplierToBuyer_OrganisationType_BuyerTypeOthers))]
    public string? OtherValue { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    private string SupplierToBuyerStateKey => $"Supplier_To_Buyer_{Id}_Answers";

    public IActionResult OnGet()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);

        BuyerOrganisationType = state.BuyerOrganisationType;

        if (!string.IsNullOrEmpty(BuyerOrganisationType) && !BuyerTypeLabels.Labels.Keys.Contains(BuyerOrganisationType))
        {
            OtherValue = BuyerOrganisationType;
            BuyerOrganisationType = "Other";
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        SupplierToBuyerStateUpdate();

        if (RedirectToSummary == true)
        {
            return RedirectToPage("SupplierToBuyerOrganisationDetailsSummary", new { Id });
        }
        else
        {
            return RedirectToPage("SupplierToBuyerDevolvedRegulation", new { Id });
        }
    }

    private void SupplierToBuyerStateUpdate()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);


        state.BuyerOrganisationType = BuyerOrganisationType;

        if (BuyerOrganisationType == "Other")
        {
            state.BuyerOrganisationOtherValue = OtherValue;
        }

        tempDataService.Put(SupplierToBuyerStateKey, state);
    }
}
