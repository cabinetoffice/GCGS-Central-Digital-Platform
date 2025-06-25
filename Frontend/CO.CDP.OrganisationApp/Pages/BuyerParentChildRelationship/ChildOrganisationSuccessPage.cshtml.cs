using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;

public class ChildOrganisationSuccessPage : PageModel
{
    [BindProperty(SupportsGet = true)]
    public required Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string OrganisationName { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        return Page();
    }
}