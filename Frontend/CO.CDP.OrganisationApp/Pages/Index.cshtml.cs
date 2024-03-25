using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnPost()
    {
        return RedirectToPage("Registration/OneLoginCallback");
    }
}