using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

[AuthorisedSessionNotRequired]
public class OrganisationInviteModel(
    ISession session) : PageModel
{
    public IActionResult OnGet(Guid personInviteId)
    {
        session.Set("PersonInviteId", personInviteId);
        return Redirect("/one-login/sign-in");
    }
}