using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class LegalFormSelectOrganisationModel(
    ITempDataService tempDataService) : PageModel
{

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public string? RegisteredOrg { get; set; }


    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public IActionResult OnGet(Guid id)
    {
        var lf = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);
        RegisteredOrg = lf.RegisteredLegalForm;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ta = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);
        ta.RegisteredLegalForm = RegisteredOrg;
        tempDataService.Put(LegalForm.TempDataKey, ta);

        return RedirectToPage("LegalFormLawRegistered", new { Id });
    }

    public static Dictionary<string, string> OrganisationLegalForm => new()
    {
        { "LimitedCompany", "Limited company"},
        { "LLP", "Limited liability partnership (LLP)"},
        { "LimitedPartnership", "Limited partnership"},
        { "Other", "Other"}
    };
}