using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ApiKeyManagement;

public class RevokeApiKeyModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public required string ApiKeyName { get; set; }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            if (!ModelState.IsValid) return Page();

            await organisationClient.RevokeAuthenticationKey(Id, ApiKeyName);

            return RedirectToPage("ManageApiKey", new { Id });
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}