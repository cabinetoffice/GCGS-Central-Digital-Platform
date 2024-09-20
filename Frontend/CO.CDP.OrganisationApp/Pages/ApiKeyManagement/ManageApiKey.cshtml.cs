using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ApiKeyManagement;

public class ManageApiKeyModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ICollection<AuthenticationKey> AuthenticationKeys { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        try
        {
            AuthenticationKeys = await organisationClient.GetAuthenticationKeys(Id);

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("CreateApiKey", new { Id });
    }
}