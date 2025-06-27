using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

public class ChildOrganisationSearchPage() : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.BuyerParentChildRelationship_SearchPage_Error))]
    public string? Query { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage("ChildOrganisationResultsPage", new { Id, query = Query });
    }
}