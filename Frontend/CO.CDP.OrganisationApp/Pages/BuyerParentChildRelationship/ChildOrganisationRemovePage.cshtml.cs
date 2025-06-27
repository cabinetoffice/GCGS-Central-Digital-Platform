using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;

public class ChildOrganisationRemovePage : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid ChildId { get; set; }

    [BindProperty]
    public bool RemoveConfirmation { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (RemoveConfirmation)
        {
            return Delete();
        }

        return RedirectToPage($"/organisation/{Id}");
    }

    private IActionResult Delete()
    {
        // TODO: Implement the logic to remove the child organisation
        return RedirectToPage($"/organisation/{Id}", new { childRemoved = true });
    }
}