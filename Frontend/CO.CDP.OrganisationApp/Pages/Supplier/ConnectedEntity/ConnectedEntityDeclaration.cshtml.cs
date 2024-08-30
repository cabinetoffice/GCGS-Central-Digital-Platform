using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedEntityDeclarationModel() : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public IActionResult OnPost()
    {
        return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
    }
}