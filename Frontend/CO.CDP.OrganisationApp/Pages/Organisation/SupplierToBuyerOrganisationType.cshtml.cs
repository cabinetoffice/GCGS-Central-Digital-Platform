using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class SupplierToBuyerOrganisationTypeModel(ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select the organisation type")]
    public string? BuyerOrganisationType { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [DisplayName("Enter type")]
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

    public static Dictionary<string, string> BuyerTypes => new()
    {
        { "CentralGovernment", "Central government, public authority: UK, Scottish, Welsh and Northern Irish Executive"},
        { "RegionalAndLocalGovernment", "Regional and local government, public authority: UK, Scottish, Welsh and Northern Irish"},
        { "PublicUndertaking", "Public undertaking"},
        { "PrivateUtility", "Private utility"}
    };

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
