using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

public abstract class LoggedInUserAwareModel : PageModel
{
    public abstract ISession SessionContext { get; }

    public UserDetails UserDetails { get; }

    public bool UserDetailsAvailable { get; } = false;

    protected LoggedInUserAwareModel()
    {
        var details = SessionContext.Get<UserDetails>(Session.UserDetailsKey);

        UserDetailsAvailable = details != null;

        UserDetails = details ?? new UserDetails { UserUrn = "" };
    }
}