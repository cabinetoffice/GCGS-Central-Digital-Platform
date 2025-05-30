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
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_BuyerOrganisationType_OtherEnterType_Label))]
    [RequiredIf("BuyerOrganisationType", "Other")]
    public string? OtherValue { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    private string SupplierToBuyerStateKey => $"Supplier_To_Buyer_{Id}_Answers";

    public IActionResult OnGet()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);

        BuyerOrganisationType = state.BuyerOrganisationType;

        if (!string.IsNullOrEmpty(BuyerOrganisationType) && !BuyerTypes.Keys.Contains(BuyerOrganisationType))
        {
            OtherValue = state.BuyerOrganisationType;
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

    public static Dictionary<string, string> BuyerTypes => new()
    {
        { "CentralGovernment", StaticTextResource.SupplierToBuyer_OrganisationType_CentralGovernment},
        { "RegionalAndLocalGovernment", StaticTextResource.SupplierToBuyer_OrganisationType_RegionalAndLocalGovernment},
        { "PublicUndertaking", StaticTextResource.SupplierToBuyer_OrganisationType_PublicUndertaking},
        { "PrivateUtility", StaticTextResource.SupplierToBuyer_OrganisationType_PrivateUtility}
    };

    private void SupplierToBuyerStateUpdate()
    {
        var state = tempDataService.PeekOrDefault<SupplierToBuyerDetails>(SupplierToBuyerStateKey);

        state.BuyerOrganisationType = (BuyerOrganisationType == "Other" ? OtherValue : BuyerOrganisationType);

        tempDataService.Put(SupplierToBuyerStateKey, state);
    }
}
