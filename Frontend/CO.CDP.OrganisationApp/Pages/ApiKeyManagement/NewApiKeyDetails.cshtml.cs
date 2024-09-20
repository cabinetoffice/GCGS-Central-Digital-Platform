using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static CO.CDP.OrganisationApp.Pages.ApiKeyManagement.CreateApiKeyModel;

namespace CO.CDP.OrganisationApp.Pages.ApiKeyManagement;

public class NewApiKeyDetailsModel() : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ApiKey { get; set; }

    public IActionResult OnGet()
    {        
        return Page();
    }
}