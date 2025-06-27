using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;

public class ChildOrganisationRemovePage : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid ChildId { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_PleaseSelect), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? RemoveConfirmation { get; set; }

    public bool HasValidationErrors => !ModelState.IsValid;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (RemoveConfirmation == true)
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