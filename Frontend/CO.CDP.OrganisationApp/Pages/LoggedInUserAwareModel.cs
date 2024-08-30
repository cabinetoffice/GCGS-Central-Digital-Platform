using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages;

public abstract class LoggedInUserAwareModel(ISession session) : PageModel
{
    public ISession SessionContext { get; } = session;

    public UserDetails UserDetails { get; }
        = session.Get<UserDetails>(Session.UserDetailsKey)
            ?? throw new Exception("Invalid session, no user details found");
}