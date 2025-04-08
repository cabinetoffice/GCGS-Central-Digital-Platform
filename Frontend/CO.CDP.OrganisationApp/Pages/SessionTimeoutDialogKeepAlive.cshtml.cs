using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class SessionTimeoutDialogKeepAliveModel(ISession session, IUserInfoService userInfoService) : LoggedInUserAwareModel(session)
{
    public IActionResult OnGet()
    {
        if(userInfoService.IsAuthenticated())
        {
            return new OkResult();
        } else
        {
            return new UnauthorizedResult();
        }
    }
}
