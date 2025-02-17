using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class LoggedOutModel : PageModel
{
    public IActionResult OnPost()
    {
        return Redirect("/one-login/sign-in");
    }
}