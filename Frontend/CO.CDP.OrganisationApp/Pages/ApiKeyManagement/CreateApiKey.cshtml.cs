using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.ApiKeyManagement;

public class CreateApiKeyModel() : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the api key name")]
    public string? ApiKeyName { get; set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        //Generate new key
        //call api and save in db (create endpoint in api for list all, create and revoke)
        return RedirectToPage("CreateApiKey", new { Id });
    }
}