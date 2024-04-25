using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OneLogin.Integration.Pages;
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return Redirect("/one-login/user-info");
        }

        return Page();
    }
}
