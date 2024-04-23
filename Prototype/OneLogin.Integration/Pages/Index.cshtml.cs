using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OneLogin.Integration.Pages;
public class IndexModel : PageModel
{
    public IActionResult OnPost()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/one-login/sign-in-callback" });
    }
}
