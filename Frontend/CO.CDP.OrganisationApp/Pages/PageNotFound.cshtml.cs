using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class PageNotFoundModel : PageModel
{
    public IActionResult OnGet()
    {
        Response.StatusCode = 404;

        return Page();
    }
}