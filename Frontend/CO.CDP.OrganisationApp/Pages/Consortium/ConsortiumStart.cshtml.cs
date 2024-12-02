using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

public class ConsortiumStartModel() : PageModel
{
    public IActionResult OnPost()
    {
        return RedirectToPage("ConsortiumName");
    }
}
