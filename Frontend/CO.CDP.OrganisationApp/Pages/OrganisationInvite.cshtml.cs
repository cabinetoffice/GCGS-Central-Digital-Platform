using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

[AuthorisedSessionNotRequired]
public class OrganisationInviteModel(
    ISession session) : PageModel
{
    [BindProperty]
    public Guid PersonInviteId {get; set;}

    public IActionResult OnGet(Guid personInviteId)
    {
        PersonInviteId = personInviteId;

        session.Set("PersonInviteId", PersonInviteId);

        return Redirect("/one-login/sign-in");
    }
}