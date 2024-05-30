using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize]
public class BasicInformationModel() : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}