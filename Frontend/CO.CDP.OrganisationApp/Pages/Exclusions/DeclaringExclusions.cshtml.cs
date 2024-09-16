using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Exclusions;

public class DeclaringExclusionsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty]
    public bool? YesNoInput { get; set; }

    public IActionResult OnPost()
    {
        if (!YesNoInput.HasValue)
            ModelState.AddModelError(nameof(YesNoInput), "Please select an option");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (YesNoInput == true)
        {
            return RedirectToPage("", new { OrganisationId, FormId, SectionId });
        }

        return RedirectToPage("../Supplier/SupplierInformationSummary", new { Id = OrganisationId });

    }
}
