using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OrganisationAlreadyRegistered: PageModel
{
    [BindProperty]
    public Guid? OrganisationId { get; set; }

    public IActionResult OnGet(Guid organisationId)
    {
        OrganisationId = organisationId;
        return Page();
    }
}