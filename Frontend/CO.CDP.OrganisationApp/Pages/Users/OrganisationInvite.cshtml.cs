using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace CO.CDP.OrganisationApp.Pages.Users;

[AuthorisedSessionNotRequired]
public class OrganisationInviteModel : PageModel
{
    public IActionResult OnGet(Guid personInviteId)
    {
        var redirectUri = $"/claim-organisation-invite/{personInviteId}";

        return Redirect($"/one-login/sign-in?redirecturi={WebUtility.UrlEncode(redirectUri)}");
    }
}