using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

[AuthorisedSessionNotRequired]
public class ErrorModel : PageModel
{
    public IActionResult OnGet()
    {
        Response.StatusCode = 500;

        return Page();
    }
}