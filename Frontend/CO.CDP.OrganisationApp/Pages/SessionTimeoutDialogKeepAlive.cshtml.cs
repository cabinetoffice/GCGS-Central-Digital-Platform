using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages;

public class SessionTimeoutDialogKeepAliveModel(ISession session) : LoggedInUserAwareModel(session)
{
    public IActionResult OnGet()
    {
        return new OkResult();
    }
}
