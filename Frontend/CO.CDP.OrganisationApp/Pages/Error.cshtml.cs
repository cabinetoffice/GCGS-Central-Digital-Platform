using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class ErrorModel : PageModel
{
    public required string TraceId { get; set; }

    public IActionResult OnGet()
    {
        Response.StatusCode = 500;
        TraceId = HttpContext.TraceIdentifier;

        return Page();
    }
}