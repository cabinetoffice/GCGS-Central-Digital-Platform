using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OrganisationAlreadyRegistered: PageModel
{
    [BindProperty]
    public string? Identifier { get; set; }

    public IActionResult OnGet(string identifier)
    {
        Identifier = identifier;
        return Page();
    }
}