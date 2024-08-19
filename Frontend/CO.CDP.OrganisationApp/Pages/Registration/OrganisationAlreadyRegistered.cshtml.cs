using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
public class OrganisationAlreadyRegistered(ISession session) : PageModel
{
    public void OnGet()
    {
    }
}