using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationSelectionModel() : PageModel
{
    public IActionResult OnPost()
    {
        return RedirectToPage("OrganisationType");
    }
}