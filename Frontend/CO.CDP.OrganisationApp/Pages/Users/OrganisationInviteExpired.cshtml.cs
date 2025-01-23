using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class OrganisationInviteExpiredModel(
    ISession session) : LoggedInUserAwareModel(session)
{
    public IActionResult OnGet()
    {
        return Page();
    }
}