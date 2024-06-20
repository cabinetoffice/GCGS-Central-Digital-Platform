using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class SupplierQualificationQuestionModel : PageModel
{

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasRelevantQualifications { get; set; }

    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (HasRelevantQualifications == true)
        {
            return RedirectToPage("SupplierQualificationAwardingBody");
        }
        else
        {
            return RedirectToPage("SupplierBasicInformation");
        }
    }
}
