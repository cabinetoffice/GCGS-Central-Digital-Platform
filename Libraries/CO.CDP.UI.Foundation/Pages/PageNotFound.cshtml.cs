using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.UI.Foundation.Pages;

public class PageNotFoundModel : PageModel
{
    public IActionResult OnGet()
    {
        Response.StatusCode = 404;

        return Page();
    }
}